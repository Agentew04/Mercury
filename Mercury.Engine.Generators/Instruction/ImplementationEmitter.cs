using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Instruction;

internal static class ImplementationEmitter {
    public static void Emit(SourceProductionContext spc, InstructionInfo instruction) {
        StringBuilder fromIntSb = new();
        foreach (FieldInfo field in instruction.Fields) {
            fromIntSb.AppendLine(string.Format(InstructionTemplates.PartialInstructionFieldExtract,
                field.FieldName,
                field.FieldType,
                field.BitStart,
                "0b" + new string('1', field.BitEnd - field.BitStart + 1)));
        }

        string toIntCode = GenerateToIntCode(instruction);

        string code = string.Format(InstructionTemplates.PartialInstruction,
            instruction.Namespace,
            instruction.ClassName,
            fromIntSb,
            toIntCode
        );
        spc.AddSource($"{instruction.Namespace}.{instruction.ClassName}.g.cs", SourceText.From(code, Encoding.UTF8));
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