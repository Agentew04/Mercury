using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Architecture;

internal static class ArchitectureManagerEmitter {
    public static void Emit(SourceProductionContext ctx, (ArchitecturesInfo Arch, ImmutableArray<GroupInfo> Groups, ImmutableArray<FlagsInfo> Flags) data) {
        (ArchitecturesInfo archs, ImmutableArray<GroupInfo> groups, ImmutableArray<FlagsInfo> flags) = data;
        StringBuilder sbArchArray = new();
        foreach (string archName in data.Arch.Architectures) {
            sbArchArray.AppendLine($"            {archs.EnumFullname}.{archName},");
        }
        
        StringBuilder sbInitCalls = new();
        foreach (string arch in archs.Architectures) {
            sbInitCalls.AppendLine($"        ArchitectureMetadata[{archs.EnumFullname}.{arch}] = Init{arch}();");
        }
        
        StringBuilder sbInitFunctions = new();
        foreach (string arch in archs.Architectures) {
            sbInitFunctions.AppendLine($"    private static ArchitectureMetadata Init{arch}() {{");
            List<GroupInfo> archGroups = groups.Where(x => x.Architecture == arch).ToList();
            IEnumerable<IGrouping<int, GroupInfo>> groupByCoProc = archGroups.GroupBy(x => x.Coprocessor);
            if (archGroups.Count == 0) {
                sbInitFunctions.AppendLine("        return new ArchitectureMetadata([]);\n    }\n");
                continue;
            }
            
            sbInitFunctions.AppendLine("        return new ArchitectureMetadata([");
            foreach (IGrouping<int, GroupInfo>? coProcGroup in groupByCoProc) {
                sbInitFunctions.Append("            new Processor(");
                sbInitFunctions.Append(coProcGroup.Key);
                sbInitFunctions.Append(", \"");
                sbInitFunctions.Append(coProcGroup.Key == 0 ? "Registers" : $"Coproc {coProcGroup.Key}");
                sbInitFunctions.AppendLine("\", [");

                foreach (GroupInfo group in coProcGroup) {
                    sbInitFunctions.Append("                new RegisterGroup(typeof(");
                    sbInitFunctions.Append(group.EnumTypeName);
                    sbInitFunctions.AppendLine("), [");
                    foreach (RegisterDef reg in group.Registers) {
                        sbInitFunctions.AppendLine(string.Format(ArchitectureTemplates.RegisterInitializationText,
                            reg.HasNumber ? reg.Number.ToString() : "-1",
                            reg.Name,
                            reg.Size.ToString(),
                            reg.IsGpr ? "true" : "false",
                            reg.EnumMemberName
                        ));
                    }
                    sbInitFunctions.AppendLine("                ]),");
                }
                sbInitFunctions.AppendLine("            ], [");
                FlagsInfo? coProcFlags = flags.FirstOrDefault(x => x.Architecture == arch && x.Processor == coProcGroup.Key);
                if (coProcFlags.HasValue) {
                    for(int i=0;i<coProcFlags.Value.FlagCount;i++) {
                        sbInitFunctions.AppendLine($"                \"{i}\",");
                    }
                }
                // no flags for now
                sbInitFunctions.AppendLine("            ]),");
            }
            sbInitFunctions.AppendLine("        ]);");
            sbInitFunctions.AppendLine("    }");
            sbInitFunctions.AppendLine();
        }

        /*
         * 0: arch array
         * 1: call inits
         * 2: init functions
         */
        string text = string.Format(ArchitectureTemplates.ArchitectureManagerText,
            sbArchArray,
            sbInitCalls,
            sbInitFunctions
        );
        
        ctx.AddSource("ArchitectureManager.g.cs", SourceText.From(text, Encoding.UTF8));
    }
}