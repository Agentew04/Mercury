using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.ModuleDescriptions;

[Generator]
internal class ModuleDescriptionGenerator : IIncrementalGenerator {

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource("ModuleDescriptionAttribute.g.cs", SourceText.From(ModuleDescriptionTemplates.ModuleDescriptionAttribute, Encoding.UTF8));
            ctx.AddSource("PropertyNameAttribute.g.cs", SourceText.From(ModuleDescriptionTemplates.PropertyNameAttribute, Encoding.UTF8));
        });

        IncrementalValuesProvider<ModuleDescriptionInfo> modules = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Mercury.Editor.Generators.Modules.ModuleDescriptionAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(x => x.HasValue)
            .Select((x,_) => x!.Value);

        IncrementalValueProvider<ImmutableArray<ModuleDescriptionInfo>> modulesCollected = modules.Collect();
        
        context.RegisterSourceOutput(modules, ModulePropertyEmitter.Emit);
        context.RegisterSourceOutput(modulesCollected, ModuleDescriptionEmitter.Emit);
    }

    private static ModuleDescriptionInfo? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext ctx) {
        ClassDeclarationSyntax cds = (ClassDeclarationSyntax)ctx.TargetNode;
        SemanticModel semanticModel = ctx.SemanticModel;
        INamedTypeSymbol? symbolInfo = semanticModel.GetDeclaredSymbol(cds);
        if (symbolInfo is null) {
            return null;
        }
        INamespaceSymbol? namespaceSymbol = symbolInfo.ContainingNamespace;

        EquatableArray<ModulePropertyInfo> props = GetProperties(symbolInfo);

        return new ModuleDescriptionInfo(
            namespaceSymbol.ToDisplayString(),
            symbolInfo.Name,
            props);
    }

    private static EquatableArray<ModulePropertyInfo> GetProperties(INamedTypeSymbol symbol) {
        ImmutableArray<ISymbol> members = symbol.GetMembers();

        List<ModulePropertyInfo> props = [];
        foreach (ISymbol? member in members) {
            if (member is not IPropertySymbol propertySymbol) {
                continue;
            }
            
            // get localization key
            string? localizationKey = null;
            ImmutableArray<AttributeData> attribs = propertySymbol.GetAttributes();
            foreach (AttributeData? attrib in attribs) {
                string fullname = attrib.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (fullname != "global::Mercury.Editor.Generators.Modules.PropertyNameAttribute") {
                    continue;
                }
                localizationKey = attrib.ConstructorArguments[0].Value as string;
                if (localizationKey != null) {
                    break;
                }
            }
            if (localizationKey == null) {
                continue;
            }

            // parse property type
            SpecialType type = propertySymbol.Type.SpecialType;
            if (type == SpecialType.None) {
                continue;
            }
            ModulePropertyType propertyType = type switch {
                SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64
                    or SpecialType.System_UInt64 => ModulePropertyType.Integer,
                SpecialType.System_Boolean => ModulePropertyType.Boolean,
                SpecialType.System_String => ModulePropertyType.String,
                _ => ModulePropertyType.Unknown
            };
            if (propertyType == ModulePropertyType.Unknown) {
                continue;
            }
            
            // add property to list
            props.Add(new ModulePropertyInfo(
                propertySymbol.Name,
                propertyType,
                localizationKey));
        }

        return new EquatableArray<ModulePropertyInfo>(props.ToArray());
    }
}