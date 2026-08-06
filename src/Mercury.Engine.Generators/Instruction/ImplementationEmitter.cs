using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Instruction;

internal static class ImplementationEmitter {
    public static void Emit(SourceProductionContext spc, InstructionInfo instruction, ImmutableArray<FormatterMethodInfo> formatters) {
        StringBuilder fromIntSb = new();
        foreach (FieldInfo field in instruction.Fields) {
            fromIntSb.AppendLine(string.Format(InstructionTemplates.PartialInstructionFieldExtract,
                field.FieldName,
                field.FieldType,
                field.BitStart,
                "0b" + new string('1', field.BitEnd - field.BitStart + 1)));
        }

        string toIntCode = GenerateToIntCode(instruction);

        string additionalMembers = "";
        if (!instruction.HasCustomToString && instruction.AssemblyFormat is not null) {
            string? toStringExpr = ConvertFormatToInterpolatedString(spc, instruction, formatters);
            if (toStringExpr is not null) {
                additionalMembers = $@"
        public override string ToString() => {toStringExpr};
";
            }
        }

        string code = string.Format(InstructionTemplates.PartialInstruction,
            instruction.Namespace,
            instruction.ClassName,
            fromIntSb,
            toIntCode,
            additionalMembers
        );
        spc.AddSource($"{instruction.Namespace}.{instruction.ClassName}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static string? ConvertFormatToInterpolatedString(
        SourceProductionContext spc,
        InstructionInfo instruction,
        ImmutableArray<FormatterMethodInfo> formatters) {

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

    private static string GenerateToIntCode(InstructionInfo instruction) {
        List<Part> parts = [];
        
        // process formats
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

            Part p = new Part() {
                Offset = format.BitStart,
                Size = format.BitEnd - format.BitStart + 1,
                IsLiteral = true,
                LiteralValue = format.Values[0]
            };
            parts.Add(p);
        }
        
        // process fields
        foreach (FieldInfo field in instruction.Fields) {
            Part p = new() {
                Offset = field.BitStart,
                Size = field.BitEnd - field.BitStart + 1,
                IsLiteral = false,
                VariableValue = field.FieldName
            };
            parts.Add(p);
        }

        if (parts.Count == 0) {
            return "        return 0;";
        }

        StringBuilder sb = new();
        sb.AppendLine("        return (uint)(");
        for (int i = 0; i < parts.Count; i++) {
            Part p = parts[i];
            sb.Append("            ");
            if (i != 0) {
                sb.Append("| ");
            }
            sb.Append('(');
            if (p.IsLiteral) {
                // calculate at compile time
                int value = (p.LiteralValue & ((1 << p.Size)-1)) << p.Offset;
                sb.Append(value.ToString());
            }
            else {
                sb.Append('(');
                sb.Append(p.VariableValue);
                sb.Append(" & 0b");
                sb.Append(new string('1', p.Size));
                sb.Append(')');
                if (p.Offset > 0) {
                    sb.Append(" << ");
                    sb.Append(p.Offset);
                }
            }
            sb.AppendLine(")");
        }

        sb.Append("        );");

        return sb.ToString();
    }

    private struct Part {
        public int Offset;
        public int Size;
        public bool IsLiteral;
        public int LiteralValue;
        public string VariableValue;
    } 
}