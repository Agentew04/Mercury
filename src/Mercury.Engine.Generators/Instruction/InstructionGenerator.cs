using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;

namespace Mercury.Generators.Instruction;


[Generator]
internal class InstructionGenerator : IIncrementalGenerator{
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource("FieldAttribute.g.cs", SourceText.From(InstructionTemplates.Attributes.FieldAttribute, Encoding.UTF8));
            ctx.AddSource("InstructionAttribute.g.cs", SourceText.From(InstructionTemplates.Attributes.InstructionAttribute, Encoding.UTF8));
            // instruction binary fmt
            ctx.AddSource("FormatExactAttribute.g.cs", SourceText.From(InstructionTemplates.Attributes.FormatExactAttributeText, Encoding.UTF8));
            ctx.AddSource("FormatDifferentAttribute.g.cs", SourceText.From(InstructionTemplates.Attributes.FormatDifferentAttribute, Encoding.UTF8));
            // instruction text format
            ctx.AddSource("AssemblyFormatAttribute.g.cs", SourceText.From(InstructionTemplates.Attributes.AssemblyFormatAttribute, Encoding.UTF8));
            ctx.AddSource("AssemblyFormatterAttribute.g.cs", SourceText.From(InstructionTemplates.Attributes.AssemblyFormatterAttribute, Encoding.UTF8));
            ctx.AddSource("AssemblyParserAttribute.g.cs", SourceText.From(InstructionTemplates.Attributes.AssemblyParserAttribute, Encoding.UTF8));
        });

        IncrementalValuesProvider<InstructionInfo> instructions = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Mercury.Engine.Generators.Instruction.InstructionAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(x => x is not null)
            .Select((x, _) => x!.Value);

        IncrementalValuesProvider<FormatterMethodInfo> formatters = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Mercury.Engine.Generators.Instruction.AssemblyFormatterAttribute",
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) => GetFormatterTargetForGeneration(ctx))
            .Where(x => x is not null)
            .Select((x, _) => x!.Value);
        
        var instructionsWithFormatters = instructions.Combine(formatters.Collect());
        
        context.RegisterSourceOutput(instructionsWithFormatters,
            (spc, pair) => {
                ImplementationEmitter.Emit(spc, pair.Left, pair.Right);
            });
        context.RegisterSourceOutput(instructions.Collect(),
            (spc, source) => {
                DisassemblerEmitter.Emit(spc, source);
                InstructionPoolEmitter.Emit(spc, source);
            });
    }

    private static FormatterMethodInfo? GetFormatterTargetForGeneration(GeneratorAttributeSyntaxContext ctx) {
        if (ctx.TargetNode is not MethodDeclarationSyntax methodDecl) {
            return null;
        }
        SemanticModel semanticModel = ctx.SemanticModel;
        IMethodSymbol? methodSymbol = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
        if (methodSymbol is null || !methodSymbol.IsStatic) {
            return null;
        }

        AttributeData? formatterAttr = methodSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == 
                         "global::Mercury.Engine.Generators.Instruction.AssemblyFormatterAttribute");
        
        if (formatterAttr is null || formatterAttr.ConstructorArguments.Length == 0) {
            return null;
        }

        string? specifier = formatterAttr.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(specifier)) {
            return null;
        }

        INamespaceSymbol? namespaceSymbol = methodSymbol.ContainingNamespace;
        INamedTypeSymbol? containingType = methodSymbol.ContainingType;

        return new FormatterMethodInfo(
            specifier!,
            namespaceSymbol.ToDisplayString(),
            containingType.Name,
            methodSymbol.Name
        );
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

        string? assemblyFormat = symbolInfo.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == 
                         "global::Mercury.Engine.Generators.Instruction.AssemblyFormatAttribute")
            ?.ConstructorArguments.FirstOrDefault().Value as string;

        bool hasCustomToString = symbolInfo.GetMembers("ToString")
            .Any(m => m is IMethodSymbol method && method.Parameters.Length == 0 && !method.IsImplicitlyDeclared);

        return new InstructionInfo(
            namespaceSymbol.ToDisplayString(), 
            symbolInfo.Name, 
            formats, fields,
            assemblyFormat,
            hasCustomToString);
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

