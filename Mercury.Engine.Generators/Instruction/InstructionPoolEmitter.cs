using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Instruction;

internal static class InstructionPoolEmitter {
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
            string text = CreatePool(group);
            string groupNamespace = group[0].Namespace;
            spc.AddSource($"{groupNamespace}InstructionPool.g.cs", SourceText.From(text, Encoding.UTF8));
        }
    }

    private static string CreatePool(ImmutableArray<InstructionInfo> instructions) {
        StringBuilder sb = new();

        foreach (InstructionInfo instruction in instructions) {
            sb.AppendLine();
            sb.AppendLine($"    public {instruction.Namespace}.{instruction.ClassName}? {instruction.ClassName};");
        }
        
        string text = string.Format(InstructionTemplates.PoolFormat,
            instructions[0].Namespace,
            sb);
        return text;
    }
}