using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.ModuleDescriptions;

internal static class ModulePropertyEmitter {
    public static void Emit(SourceProductionContext spc, ModuleDescriptionInfo info) {
        string function = CreateFunction(info);
        string source = string.Format(ModuleDescriptionTemplates.ModuleDescriptionClassFormat,
            info.Namespace,
            info.ClassName,
            function);
        spc.AddSource($"{info.ClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string CreateFunction(ModuleDescriptionInfo info) {
        StringBuilder sbProps = new();
        StringBuilder sbArray = new();

        foreach (ModulePropertyInfo prop in info.Properties) {
            string uncapitalized = prop.Name.ToLower();
            if (prop.Name == uncapitalized) {
                uncapitalized = '_' + prop.Name;
            }
            sbProps.AppendLine(string.Format(ModuleDescriptionTemplates.GetPropertyInfoFormat,
                uncapitalized, prop.Name));

            string propType = prop.Type switch {
                ModulePropertyType.Integer => "IntegerModuleProperty",
                ModulePropertyType.Boolean => "BooleanModuleProperty",
                ModulePropertyType.String => "StringModuleProperty",
                _ => "object"
            };

            sbArray.AppendLine(string.Format(ModuleDescriptionTemplates.CreatePropertyFormat,
                propType, prop.LocalizationKey, uncapitalized));
        }

        return string.Format(ModuleDescriptionTemplates.GetSpecificPropertiesFormat,
            sbProps,
            sbArray
        );
    }
}