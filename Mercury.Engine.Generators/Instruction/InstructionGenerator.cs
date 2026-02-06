using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Instruction;


[Generator]
internal class InstructionGenerator : IIncrementalGenerator{
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource("FieldAttribute.g.cs", SourceText.From(InstructionTemplates.FieldAttribute, Encoding.UTF8));
            ctx.AddSource("InstructionAttribute.g.cs", SourceText.From(InstructionTemplates.InstructionAttribute, Encoding.UTF8));
            ctx.AddSource("FormatExactAttribute.g.cs", SourceText.From(InstructionTemplates.FormatExactAttributeText, Encoding.UTF8));
            ctx.AddSource("FormatDifferentAttribute.g.cs", SourceText.From(InstructionTemplates.FormatDifferentAttribute, Encoding.UTF8));
        });

        IncrementalValuesProvider<InstructionInfo> instructions = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Mercury.Engine.Generators.Instruction.InstructionAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(x => x is not null)
            .Select((x, _) => x!.Value);
        
        context.RegisterSourceOutput(instructions,
            (spc, source) => {
                ImplementationEmitter.Emit(spc,source);
            });
        context.RegisterSourceOutput(instructions.Collect(),
            (spc, source) => {
                DisassemblerEmitter.Emit(spc,source);
            });
    }

    private static InstructionInfo? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext ctx) {
        TypeDeclarationSyntax type = (TypeDeclarationSyntax)ctx.TargetNode;
        SemanticModel semanticModel = ctx.SemanticModel;
        INamedTypeSymbol? symbolInfo = semanticModel.GetDeclaredSymbol(type);
        if (symbolInfo is null) {
            return null;
        }
        INamespaceSymbol? namespaceSymbol = symbolInfo.ContainingNamespace;

        EquatableArray<FormatInfo> formats = GetFormats(symbolInfo);
        EquatableArray<FieldInfo> fields = GetFields(symbolInfo);

        return new InstructionInfo(
            namespaceSymbol.ToDisplayString(), 
            symbolInfo.Name, 
            formats, fields);
    }

    private static EquatableArray<FormatInfo> GetFormats(INamedTypeSymbol symbol) {
        List<FormatInfo> formats = [];
        foreach (AttributeData attribute in symbol.GetAttributes()) {
            string fullname = attribute.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            bool isExact = fullname == "global::Mercury.Engine.Generators.Instruction.FormatExactAttribute";
            bool isDiff = fullname == "global::Mercury.Engine.Generators.Instruction.FormatDifferentAttribute";
            if (!isExact && !isDiff) {
                continue;
            }
            
            if (attribute.ConstructorArguments[0].Value is not int) {
                continue;
            }
            if (attribute.ConstructorArguments[1].Value is not int) {
                continue;
            }
            
            int bitStart = (int)attribute.ConstructorArguments[0].Value!;
            int bitEnd = (int)attribute.ConstructorArguments[1].Value!;

            List<int> values = [];
            if (attribute.ConstructorArguments[2].Kind == TypedConstantKind.Array) {
                foreach (TypedConstant value in attribute.ConstructorArguments[2].Values) {
                    values.Add((int)value.Value!);
                }
            }
            else if (attribute.ConstructorArguments[2].Value is int) {
                values.Add((int)attribute.ConstructorArguments[2].Value!);
            }
            else {
                continue;
            }

            if (bitEnd < bitStart) {
                (bitStart, bitEnd) = (bitEnd, bitStart);
            }
            
            FormatInfoType infoType;
            if (isExact) {
                infoType = FormatInfoType.Exact;
            }else if (isDiff) {
                infoType = FormatInfoType.Different;
            }
            else {
                infoType = FormatInfoType.Unknown;
            }
            formats.Add(new FormatInfo(infoType, bitStart, bitEnd, values));
        }

        return new EquatableArray<FormatInfo>(formats.ToArray());
    }

    private static EquatableArray<FieldInfo> GetFields(INamedTypeSymbol symbolInfo) {
        List<FieldInfo> fields = [];
        foreach (ISymbol? member in symbolInfo.GetMembers()) {
            if (member is not IFieldSymbol && member is not IPropertySymbol) {
                continue;
            }
            
            AttributeData? fieldAttribute = member
                .GetAttributes()
                .FirstOrDefault(x => x.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                     == "global::Mercury.Engine.Generators.Instruction.FieldAttribute");
            if (fieldAttribute is null) {
                continue;
            }

            int bitStart = (int)fieldAttribute.ConstructorArguments[0].Value!;
            int bitEnd = (int)fieldAttribute.ConstructorArguments[1].Value!;
            if (bitStart > bitEnd) {
                (bitStart, bitEnd) = (bitEnd, bitStart);
            }

            string fieldtype;
            if (member is IFieldSymbol field) {
                fieldtype = field.Type.Name;
            }
            else {
                var prop = (IPropertySymbol)member;
                fieldtype = prop.Type.Name;
            }
            fields.Add(new FieldInfo(bitStart, bitEnd, fieldtype, member.Name));
        }
        return new EquatableArray<FieldInfo>(fields.ToArray());
    }
    
}

