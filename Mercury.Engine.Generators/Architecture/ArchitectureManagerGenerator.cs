using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mercury.Generators.Registers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mercury.Generators.Architecture;

[Generator]
public class ArchitectureManagerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext ctx) {
        // send attributes and base classes
        ctx.RegisterPostInitializationOutput(pic => {
            pic.AddSource("ArchitectureMetadata.g.cs", ArchitectureTemplates.ArchitectureMetadataText);
            pic.AddSource("Processor.g.cs", ArchitectureTemplates.ProcessorText);
            pic.AddSource("RegisterGroup.g.cs", ArchitectureTemplates.RegisterGroupText);
            pic.AddSource("RegisterDefinition.g.cs", ArchitectureTemplates.RegisterDefinitionText);
            pic.AddSource("ArchitectureAttribute.g.cs", ArchitectureTemplates.ArchitectureAttributeText);
            pic.AddSource("InvalidAttribute.g.cs", ArchitectureTemplates.InvalidAttributeText);
            pic.AddSource("ProcessorFlagsAttribute.g.cs", ArchitectureTemplates.ProcessorFlagsAttributeText);
        });

        // read values from source code
        IncrementalValueProvider<ImmutableArray<GroupInfo>> groupList = ctx.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Mercury.Generators.RegisterGroupDefinitionAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetRegisterGroup(ctx)).Collect();

        IncrementalValueProvider<ImmutableArray<ArchitecturesInfo>> archList =
            ctx.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Mercury.Generators.ArchitectureAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetArchitecture(ctx)).Collect();

        IncrementalValueProvider<ImmutableArray<FlagsInfo>> flagsList = ctx.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Mercury.Generators.ProcessorFlagsAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetFlags(ctx)).Collect();

        IncrementalValueProvider<(ImmutableArray<GroupInfo> Groups,ArchitecturesInfo Arch)> groupAndArch = groupList.Combine(archList)
            .Select((x,_) => (x.Left,x.Right[0]));
        
        IncrementalValueProvider<(ArchitecturesInfo Arch, ImmutableArray<GroupInfo> Groups, ImmutableArray<FlagsInfo> Flags)> groupArchAndFlags = groupAndArch.Combine(flagsList)
            .Select((x,_) => (x.Left.Arch, x.Left.Groups, x.Right));
        
        ctx.RegisterSourceOutput(groupArchAndFlags,
            (spc, source) => {
                ArchitectureManagerEmitter.Emit(spc, source);
            });
    }

    private static GroupInfo GetRegisterGroup(GeneratorAttributeSyntaxContext ctx) {
        // get coprocessor number
        int coprocessor = (int)ctx.Attributes[0].NamedArguments.First(x => x.Key == "Processor").Value.Value!;
        
        // get architecture name
        int archIndex = (int)ctx.Attributes[0].ConstructorArguments[0].Value!;
        string architecture = ctx.Attributes[0].ConstructorArguments[0].Type!.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(x => x.HasConstantValue && (int)x.ConstantValue! == archIndex)?.Name ?? "null";
        
        string enumTypeName = ctx.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        string? processorName =
            ctx.Attributes[0].NamedArguments.FirstOrDefault(x => x.Key == "ProcessorName").Value.Value as string;

        string groupName = (string)ctx.Attributes[0].NamedArguments.First(x => x.Key == "Name").Value.Value!;

        List<RegisterInfo> registers = [];
        
        EnumDeclarationSyntax eds = (EnumDeclarationSyntax)ctx.TargetNode;
        foreach (EnumMemberDeclarationSyntax member in eds.Members) {
            IFieldSymbol? field = (IFieldSymbol?)ctx.SemanticModel.GetDeclaredSymbol(member);

            AttributeData? attribute = field?.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass!.ToDisplayString() == "Mercury.Generators.RegisterAttribute");
            if (attribute is null || field is null) {
                continue;
            }

            int number = -1;
            int index = 0;
            if (attribute.ConstructorArguments.Length == 4) {
                number = (int)attribute.ConstructorArguments[0].Value!;
                index++;
            }
            
            registers.Add(new  RegisterInfo(
                field.ToDisplayString(),
                (string)attribute.ConstructorArguments[index].Value!,
                number != -1,
                number,
                (int)attribute.ConstructorArguments[index+1].Value!,
                (bool)attribute.ConstructorArguments[index+2].Value!
                ));
        }
        
        return new GroupInfo(architecture, coprocessor, enumTypeName, registers, processorName, groupName);
    }

    private static ArchitecturesInfo GetArchitecture(GeneratorAttributeSyntaxContext ctx) {
        string fullname = ctx.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        // pegar campos do enum
        EnumDeclarationSyntax eds = (EnumDeclarationSyntax)ctx.TargetNode;
        List<string> archs = [];
        foreach (EnumMemberDeclarationSyntax member in eds.Members) {
            IFieldSymbol? field = (IFieldSymbol?)ctx.SemanticModel.GetDeclaredSymbol(member);
            if (field is null) {
                continue;
            }
            
            // check if is marked as invalid
            bool invalid = field.GetAttributes()
                .Any(x => x.AttributeClass!.ToDisplayString() == "Mercury.Generators.InvalidAttribute");
            if (!invalid) {
                archs.Add(field.Name);
            }
        }
        return new ArchitecturesInfo(fullname, archs);
    }

    private static FlagsInfo GetFlags(GeneratorAttributeSyntaxContext ctx) {
        
        // get architecture name
        int archIndex = (int)ctx.Attributes[0].ConstructorArguments[0].Value!;
        string architecture = ctx.Attributes[0].ConstructorArguments[0].Type!.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(x => x.HasConstantValue && (int)x.ConstantValue! == archIndex)?.Name ?? "null";
        int processor = (int)ctx.Attributes[0].NamedArguments.First(x => x.Key == "Processor").Value.Value!;
        
        string enumTypeName = ctx.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        EnumDeclarationSyntax eds = (EnumDeclarationSyntax)ctx.TargetNode;
        int flagCount = eds.Members.Count;
        
        return new FlagsInfo(architecture, enumTypeName, flagCount, processor);
    }

}