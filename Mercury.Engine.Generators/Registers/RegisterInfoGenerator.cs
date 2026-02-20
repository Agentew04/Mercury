using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Registers;

internal readonly record struct RegisterInfo {
    public readonly bool HasNumber;
    public readonly int Number;
    public readonly string Name;
    public readonly string EnumMemberName;
    public readonly int Size;
    public readonly bool IsGpr;

    public RegisterInfo(string enumMemberName, string name, bool hasNumber, int number, int size, bool isGpr) {
        EnumMemberName = enumMemberName;
        Number = number;
        HasNumber = hasNumber;
        Name = name;
        Size = size;
        IsGpr = isGpr;
    }
}

internal readonly record struct EnumToGenerate {
    public readonly EquatableArray<RegisterInfo> Registers;
    public readonly string FullEnumName;
    public readonly string ShortEnumName;
    public readonly string ArchitectureFieldName;
    public readonly string FullArchitectureClassName;
    public readonly string? ProcessorName;

    public EnumToGenerate(string enumName, string shortname, List<RegisterInfo> regs, string architectureFieldName, string fullArchitectureClassName) {
        Registers = new EquatableArray<RegisterInfo>(regs.ToArray());
        FullEnumName = enumName;
        ShortEnumName = shortname;
        ArchitectureFieldName = architectureFieldName;
        FullArchitectureClassName = fullArchitectureClassName;
    }
}

[Generator]
public class RegisterInfoGenerator : IIncrementalGenerator{
    
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource("RegisterGroupDefinitionAttribute.g.cs", SourceText.From(RegisterInfoTemplates.RegisterGroupAttribute, Encoding.UTF8));
            ctx.AddSource("RegisterAttribute.g.cs", SourceText.From(RegisterInfoTemplates.RegisterAttributeText, Encoding.UTF8));
        });
        
        IncrementalValuesProvider<EnumToGenerate> enums = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Mercury.Generators.RegisterGroupDefinitionAttribute",
            predicate: static (_, _) => true,
            transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)
        );
        
        context.RegisterSourceOutput(enums, RegisterHelperEmitter.Emit);
        context.RegisterSourceOutput(enums.Collect(), RegisterHelperProviderEmitter.Emit);
    }

    private static EnumToGenerate GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext ctx) {
        var eds = (EnumDeclarationSyntax)ctx.TargetNode;
        var enumSymbol = (ITypeSymbol?)ModelExtensions.GetDeclaredSymbol(ctx.SemanticModel, eds);
        
        // get architecture
        int archIndex = (int)ctx.Attributes[0].ConstructorArguments[0].Value!;
        string architectureFieldName = ctx.Attributes[0].ConstructorArguments[0].Type!.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.HasConstantValue && (int)f.ConstantValue! == archIndex)?.Name ?? "null";
        string fullArchitectureClassName = ctx.Attributes[0].ConstructorArguments[0].Type!
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        // get register definitons
        List<RegisterInfo> regs = [];
        for (int i = 0; i < eds.Members.Count; i++) {
            EnumMemberDeclarationSyntax member = eds.Members[i];
            IFieldSymbol? fieldSymbol = (IFieldSymbol?)ModelExtensions.GetDeclaredSymbol(ctx.SemanticModel, member);
            if (fieldSymbol is null) {
                continue;
            }
            ImmutableArray<AttributeData> attributes = fieldSymbol.GetAttributes();
            foreach (AttributeData attribute in attributes) {
                if (attribute.AttributeClass!.ToDisplayString() != "Mercury.Generators.RegisterAttribute") continue;
                if (attribute.ConstructorArguments.Length == 4) {
                    regs.Add(new RegisterInfo(fieldSymbol.ToDisplayString(), (string)attribute.ConstructorArguments[1].Value!, true, (int)attribute.ConstructorArguments[0].Value!, (int)attribute.ConstructorArguments[2].Value!, (bool)attribute.ConstructorArguments[3].Value!));
                }
                else {
                    regs.Add(new RegisterInfo(fieldSymbol.ToDisplayString(), (string)attribute.ConstructorArguments[0].Value!, false, -1, (int)attribute.ConstructorArguments[1].Value!, (bool)attribute.ConstructorArguments[2].Value!));
                }
                break;
            }
        }

        return new EnumToGenerate(enumSymbol.ToDisplayString(), enumSymbol.Name, regs, architectureFieldName, fullArchitectureClassName);
    }
}