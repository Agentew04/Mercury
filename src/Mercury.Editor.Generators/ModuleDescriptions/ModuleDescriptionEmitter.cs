using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.ModuleDescriptions;

internal static class ModuleDescriptionEmitter {
    public static void Emit(SourceProductionContext spc, ImmutableArray<ModuleDescriptionInfo> modules) {

        StringBuilder sbList = new();

        foreach (ModuleDescriptionInfo module in modules) {
            sbList.AppendLine(string.Format(ModuleDescriptionTemplates.BaseModuleDescriptionTypeofFormat,
                module.Namespace+"."+module.ClassName));
        }
        
        string source = string.Format(ModuleDescriptionTemplates.BaseModuleDescriptionFormat,
            sbList
        );
        
        spc.AddSource("ModuleDescription.g.cs", SourceText.From(source, Encoding.UTF8));
    }
}