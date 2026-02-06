using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Instruction;

/// <summary>
/// Emits the Disassembler class and method.
/// </summary>
/// <remarks>Instructions are grouped by namespace, so instructions sets must share
/// the same namespace.</remarks>
internal static class DisassemblerEmitter {

    public static void Emit(SourceProductionContext spc, ImmutableArray<InstructionInfo> instructions) {
        ImmutableArray<ImmutableArray<InstructionInfo>> groups = instructions
            .GroupBy(i => i.Namespace)
            .Select(g => g.ToImmutableArray())
            .ToImmutableArray();

        if (groups.Length == 0) {
            return;
        }
        
        foreach (ImmutableArray<InstructionInfo> group in groups) {
            if (group.Length == 0) {
                continue;
            }
            string text = CreateDisassembler(group);
            string groupNamespace = group[0].Namespace;
            spc.AddSource($"{groupNamespace}Disassembler.g.cs", SourceText.From(text, Encoding.UTF8));
        }
    }

    private static string CreateDisassembler(ImmutableArray<InstructionInfo> instructions) {
        StringBuilder sb = new();

        foreach (InstructionInfo instruction in instructions) {
            if (instruction.Formats.Count == 0) {
                continue;
            }

            WriteDocumentation(sb,instruction);
            WriteCondition(sb,instruction); // writes: "if(...) {"
            sb.AppendLine($"            pool.{instruction.ClassName} ??= new();");
            sb.AppendLine($"            return pool.{instruction.ClassName};");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        /*
         * 0: namespace
         * 1: return type
         * 2: method body
         * 3: return type
         * 4: return type
         */
        string text = string.Format(InstructionTemplates.DisassemblerFormat,
            instructions[0].Namespace,
            instructions.Length,
            sb);
        return text;
    }

    private static void WriteDocumentation(StringBuilder sb, InstructionInfo instruction) {
        sb.Append("        // Instruction: ");
        sb.AppendLine(instruction.ClassName);
        sb.Append("        // constraints: ");
        bool firstFormat = true;
        foreach (FormatInfo format in instruction.Formats) {
            if (!firstFormat) {
                sb.Append(" AND ");
            }

            if (format.InfoType == FormatInfoType.Different) {
                sb.Append(" NOT ");
            }

            firstFormat = false;
            sb.Append($"bits {format.BitStart}-{format.BitEnd} in (");
            bool firstValue = true;
            foreach (int value in format.Values) {
                if (!firstValue) {
                    sb.Append(", ");
                }

                firstValue = false;
                sb.Append($"0x{value:X}");
            }

            sb.Append(")");
        }

        sb.AppendLine();
    }

    private static void WriteCondition(StringBuilder sb, InstructionInfo instruction) {
        sb.Append("        if(");
        for (int i = 0; i < instruction.Formats.Count; i++) {
            FormatInfo formatInfo = instruction.Formats[i];

            if (formatInfo.Values.Count > 1) {
                if (formatInfo.InfoType == FormatInfoType.Different) {
                    sb.Append('!');
                }
                sb.Append('(');
            }

            for (int j = 0; j < formatInfo.Values.Count; j++) {
                int value = formatInfo.Values[j];
                if (j > 0) {
                    sb.Append(" || ");
                }

                sb.Append('(');
                sb.Append(formatInfo.BitStart != 0 ? $"(instruction >> {formatInfo.BitStart}) " : "instruction ");
                sb.Append($"& 0b{new string('1', formatInfo.BitEnd-formatInfo.BitStart+1)}) {(
                    formatInfo.InfoType == FormatInfoType.Different && formatInfo.Values.Count == 1 ? '!' : '='
                )}= 0x{value:X}");
            }
                
            if (formatInfo.Values.Count > 1) {
                sb.Append(')');
            }
                
            if(i < instruction.Formats.Count - 1) {
                sb.Append("\n            && ");
            }
        }
            
        sb.AppendLine(") {");
    }
}