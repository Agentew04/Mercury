using System.Buffers.Binary;
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
        
        /*
         * Faz disassemble das menores instrucoes ateh as maiores.
         */
            
        
        int[] sizes = instructions
            .Where(i => i.Formats.Count > 0)
            .Select(i => i.Size)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        int minWidth;
        int maxWidth;
        int widthStep;
        if (sizes.Length == 0) {
            minWidth = 4;
            maxWidth = 4;
            widthStep = 4;
        }
        else {
            minWidth = sizes[0];
            maxWidth = sizes[sizes.Length - 1];
            widthStep = sizes.Aggregate(0, Mdc); // maximo step entre os tamanhos (MDC)
        }

        StringBuilder sb = new();
        foreach (int size in sizes) {
            ImmutableArray<InstructionInfo> group = instructions
                .Where(i => i.Size == size && i.Formats.Count > 0)
                .ToImmutableArray();
            WriteSizeBracket(sb, size, group);
        }

        /*
         * 0: namespace
         * 1: instruction count
         * 2: min width (bytes)
         * 3: max width (bytes)
         * 4: width step (bytes)
         * 5: GetType body
         */
        string text = string.Format(InstructionTemplates.DisassemblerFormat,
            instructions[0].Namespace,
            instructions.Length,
            minWidth,
            maxWidth,
            widthStep,
            sb);
        return text;
    }

    /// <summary>
    /// Calcula o maximo divisor comum de dois numeros.
    /// </summary>
    private static int Mdc(int a, int b) {
        while (b != 0) {
            (a, b) = (b, a % b);
        }
        return a;
    }

    private static void WriteSizeBracket(StringBuilder sb, int size, ImmutableArray<InstructionInfo> instructions) {
        if (instructions.Length == 0) {
            return;
        }

        sb.AppendLine($"        if (widthBytes == {size}) {{");

        // add uint32 instruction if it fits  
        if (size <= 4) {
            sb.AppendLine("            uint instruction = endianness == global::Mercury.Engine.Memory.Endianness.LittleEndian ? ");
            sb.AppendLine("                global::System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes) : ");
            sb.AppendLine("                global::System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes);");
            sb.AppendLine();
        }
        foreach (InstructionInfo instruction in instructions) {
            WriteDocumentation(sb, instruction);
            WriteCondition(sb, instruction); // writes: "if(...) {"
            sb.AppendLine($"                pool.{instruction.ClassName} ??= new();");
            sb.AppendLine($"                return pool.{instruction.ClassName};");
            sb.AppendLine("            }");
            sb.AppendLine();
        }
        sb.AppendLine("        }");
        sb.AppendLine();
    }
    
    private static void WriteDocumentation(StringBuilder sb, InstructionInfo instruction) {
        sb.Append("            // Instruction: ");
        sb.AppendLine(instruction.ClassName);
        sb.Append("            // constraints: ");
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
        sb.Append("            if(");
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

                bool notEquals = formatInfo.InfoType == FormatInfoType.Different && formatInfo.Values.Count == 1;
                // troca do antigo bitwise para esse helper.
                // bitwise seria mt complicado para instrucoes maiores que 64 bits (ulong).
                // o helper 
                
                // get bits
                sb.Append('(');
                if (instruction.Size == 4) {
                    // 32 bit bitwise fast path
                    int bitCount = formatInfo.BitEnd - formatInfo.BitStart + 1;
                    uint mask = bitCount == 32 ? uint.MaxValue : (1u << bitCount) - 1;
                    sb.Append("((instruction >> ");
                    sb.Append(formatInfo.BitStart);
                    sb.Append(") & 0x");
                    sb.Append(mask.ToString("X"));
                    sb.Append("U)");
                }
                else {
                    // generic
                    sb.Append("global::Mercury.Engine.Common.InstructionBits.ReadBitsFast(bytes, widthBytes, ");
                    sb.Append(formatInfo.BitStart);
                    sb.Append(", ");
                    sb.Append(formatInfo.BitEnd);
                    sb.Append(", endianness) ");
                }
                
                // compare
                sb.Append(notEquals ? "!=" : "==");
                
                // value
                sb.Append(" 0x");
                sb.Append(value.ToString("X"));
                sb.Append(')');
            }
                
            if (formatInfo.Values.Count > 1) {
                sb.Append(')');
            }
                
            if(i < instruction.Formats.Count - 1) {
                sb.Append("\n                && ");
            }
        }
            
        sb.AppendLine(") {");
    }
}