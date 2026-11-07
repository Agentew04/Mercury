using System;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Instruction;

internal static class ImplementationEmitter {
    public static void Emit(SourceProductionContext spc, InstructionInfo instruction, ImmutableArray<FormatterMethodInfo> formatters) {
        if (instruction.WidthBits <= 0 || instruction.WidthBits % 8 != 0) {
            spc.ReportDiagnostic(Diagnostic.Create(
                InstructionDiagnostics.InvalidInstructionWidth,
                Location.None,
                instruction.WidthBits));
        }

        string fromBytesCode = GenerateFromBytesBody(instruction);
        
        string toBytesCode = GenerateToBytesBody(instruction);

        StringBuilder sbAdditionalMembers = new();
        if (!instruction.HasCustomToString && instruction.AssemblyFormat is not null) {
            string? toStringExpr = ConvertFormatToInterpolatedString(instruction, formatters);
            if (toStringExpr is not null) {
                sbAdditionalMembers.AppendLine($"    public override string ToString() => {toStringExpr};");
            }
        }

        sbAdditionalMembers.AppendLine(
            """
                public uint ConvertToInt(global::Mercury.Engine.Memory.Endianness endianness) {
                    Span<byte> buffer = stackalloc byte[4];
                    ToBytes(buffer, endianness);
                    if(endianness == global::Mercury.Engine.Memory.Endianness.BigEndian){
                        return global::System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(buffer);
                    }else{
                        return global::System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(buffer);
                    }
                }

            """
        );

        string code = string.Format(InstructionTemplates.PartialInstruction,
            instruction.Namespace,
            instruction.ClassName,
            instruction.Size,
            fromBytesCode,
            toBytesCode,
            sbAdditionalMembers
        );
        spc.AddSource($"Mercury.Generators/Instructions/{instruction.Namespace}/{instruction.ClassName}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateFromBytesBody(InstructionInfo instruction) {
        StringBuilder sb = new();
        foreach (FieldInfo field in instruction.Fields) {
            if (field.IsSigned) {
                sb.AppendLine(string.Format(InstructionTemplates.PartialInstructionSignedFieldExtract,
                    field.FieldName,
                    field.FieldType,
                    field.BitStart,
                    field.BitEnd,
                    field.BitEnd - field.BitStart + 1));
            }
            else {
                sb.AppendLine(string.Format(InstructionTemplates.PartialInstructionFieldExtract,
                    field.FieldName,
                    field.FieldType,
                    field.BitStart,
                    field.BitEnd));
            }
        }

        return sb.ToString();
    }
    
    /// <summary>
    /// Builds the <c>ToBytes</c> body: clears the destination and writes every format literal and
    /// field through <see cref="InstructionBits.WriteBits"/>. Works for instructions of any width.
    /// </summary>
    private static string GenerateToBytesBody(InstructionInfo instruction) {
        StringBuilder sb = new();
        sb.AppendLine("        bytes.Clear();");

        // format literals
        foreach (FormatInfo format in instruction.Formats) {
            if (format.InfoType != FormatInfoType.Exact) {
                continue;
            }
            if (format.Values.Count != 1) {
                continue;
            }
            if (format.Values[0] == 0) {
                continue;
            }
            sb.AppendLine(string.Format(
                "        global::Mercury.Engine.Common.InstructionBits.WriteBitsFast(bytes, Size, {0}, {1}, {2}UL, endianness);",
                format.BitStart,
                format.BitEnd,
                (uint)format.Values[0]));
        }

        // fields
        foreach (FieldInfo field in instruction.Fields) {
            sb.AppendLine(string.Format(
                "        global::Mercury.Engine.Common.InstructionBits.WriteBitsFast(bytes, Size, {0}, {1}, (global::System.UInt64)({2}), endianness);",
                field.BitStart,
                field.BitEnd,
                field.FieldName));
        }

        return sb.ToString();
    }

    private static string? ConvertFormatToInterpolatedString(InstructionInfo instruction, ImmutableArray<FormatterMethodInfo> formatters) {

        /*
         * Formato esperado:
         *
         * (.|{PROP(:FMT)?})+
         */
        
        string format = instruction.AssemblyFormat!;
        StringBuilder sb = new();
        sb.Append("$\"");
        int i = 0;
        while (i < format.Length) {
            if (format[i] == '{') {
                if (i + 1 < format.Length && format[i + 1] == '{') {
                    sb.Append("{{");
                    i += 2;
                    continue;
                }
                int closeIndex = format.IndexOf('}', i);
                if (closeIndex == -1) {
                    // Malformed brace — abort; analyzer (MRCY0007) already reports this
                    return null;
                }
                string content = format.Substring(i + 1, closeIndex - i - 1);
                int colonIndex = content.IndexOf(':');
                if (colonIndex == -1) {
                    sb.Append('{').Append(content).Append('}');
                } else {
                    string varName = content.Substring(0, colonIndex).Trim();
                    string specifier = content.Substring(colonIndex + 1).Trim();
                    
                    // Check if specifier matches a registered AssemblyFormatter method
                    FormatterMethodInfo? formatterMatch = FindFormatter(specifier, instruction.Namespace, formatters);
                    if (formatterMatch is not null) {
                        sb.Append("{(global::").Append(formatterMatch.Value.Namespace)
                          .Append('.').Append(formatterMatch.Value.ClassName)
                          .Append('.').Append(formatterMatch.Value.MethodName)
                          .Append('(').Append(varName).Append("))}");
                    } else if (IsValidStandardFormatSpecifier(specifier)) {
                        // Standard .NET format specifier (e.g. X4, D, B8) — keep as-is
                        sb.Append('{').Append(content).Append('}');
                    } else {
                        // Unknown specifier — abort; analyzer (MRCY0006) already reports this
                        return null;
                    }
                }
                i = closeIndex + 1;
            } else if (format[i] == '}') {
                if (i + 1 < format.Length && format[i + 1] == '}') {
                    sb.Append("}}");
                    i += 2;
                } else {
                    sb.Append('}');
                    i++;
                }
            } else if (format[i] == '"') {
                sb.Append("\\\"");
                i++;
            } else {
                sb.Append(format[i]);
                i++;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    private static FormatterMethodInfo? FindFormatter(
        string specifier,
        string instructionNamespace,
        ImmutableArray<FormatterMethodInfo> formatters) {
        
        string ns = instructionNamespace;
        while (!string.IsNullOrEmpty(ns)) {
            foreach (var f in formatters) {
                if (f.Specifier == specifier && f.Namespace == ns) {
                    return f;
                }
            }
            int lastDot = ns.LastIndexOf('.');
            if (lastDot == -1) {
                break;
            }
            ns = ns.Substring(0, lastDot);
        }
        
        foreach (var f in formatters) {
            if (f.Specifier == specifier) {
                return f;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns true when <paramref name="specifier"/> is a recognised standard .NET numeric/format letter
    /// (one letter from the set X/D/B/G/F/N, optionally followed by decimal precision digits).
    /// </summary>
    private static bool IsValidStandardFormatSpecifier(string specifier) {
        if (string.IsNullOrEmpty(specifier)) {
            return true;
        }
        char first = specifier[0];
        if (first == 'X' || first == 'x' || first == 'D' || first == 'd' ||
            first == 'B' || first == 'b' || first == 'G' || first == 'g' ||
            first == 'F' || first == 'f' || first == 'N' || first == 'n') {
            for (int j = 1; j < specifier.Length; j++) {
                if (!char.IsDigit(specifier[j])) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
}