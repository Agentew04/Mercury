using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mercury.Generators.Instruction;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class InstructionAnalyzer : DiagnosticAnalyzer {
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        InstructionDiagnostics.ImplementInterface,
        InstructionDiagnostics.UsePartial,
        InstructionDiagnostics.FieldNoAttribute,
        InstructionDiagnostics.InsufficientFieldSize,
        InstructionDiagnostics.FormattingAmbiguity
    ];
    
    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInterfaceUsage, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePartialClass, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeFieldAttribute, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeFieldSize, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeCoverage, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeInterfaceUsage(SyntaxNodeAnalysisContext context) {
        ClassDeclarationSyntax classDecl = (ClassDeclarationSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;
        INamedTypeSymbol? symbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (symbol is null) {
            return;
        }

        bool hasInstructionAttribute = symbol.GetAttributes()
            .Any(attr => attr.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == 
                         "global::Mercury.Engine.Generators.Instruction.InstructionAttribute");

        if (!hasInstructionAttribute) {
            return;
        }
        
        // check if implements interface
        bool hasInterface = symbol.AllInterfaces.Any(x => 
            x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Mercury.Engine.Common.IInstruction");

        if (hasInterface) {
            return;
        }
        var diagnostic = Diagnostic.Create(
            InstructionDiagnostics.ImplementInterface,
            classDecl.BaseList?.GetLocation() ?? classDecl.Identifier.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
    
    private static void AnalyzePartialClass(SyntaxNodeAnalysisContext context) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;
        if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol symbol) {
            return;
        }
        bool hasInstructionAttribute = symbol.GetAttributes()
            .Any(attr => attr.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) 
                         == "global::Mercury.Engine.Generators.Instruction.InstructionAttribute");
        if (!hasInstructionAttribute) {
            return;
        }
    
        SyntaxToken partial = classDeclarationSyntax.Modifiers.FirstOrDefault(x => x.IsKind(SyntaxKind.PartialKeyword));
        if (partial != default) {
            return;
        }
    
        var diagnostic = Diagnostic.Create(
            InstructionDiagnostics.UsePartial,
            classDeclarationSyntax.Keyword.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeFieldAttribute(SyntaxNodeAnalysisContext context) {
        if (context.Node is not FieldDeclarationSyntax && context.Node is not PropertyDeclarationSyntax) {
            return;
        }

        SemanticModel semanticModel = context.SemanticModel;
        ISymbol? symbol = semanticModel.GetDeclaredSymbol(context.Node);

        if (symbol is not IFieldSymbol && symbol is not IPropertySymbol) {
            return;
        }
        bool hasFieldAttrib = symbol.GetAttributes().Any(x =>
            x.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
            "global::Mercury.Engine.Generators.Instruction.FieldAttribute");

        if (!hasFieldAttrib) {
            return;
        }
        // check parent
        bool hasInstructionAttribute = symbol.ContainingType.GetAttributes()
            .Any(attr => attr.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) 
                         == "global::Mercury.Engine.Generators.Instruction.InstructionAttribute");
        if (hasInstructionAttribute) {
            return;
        }
        
        Location loc;
        switch (context.Node) {
            case FieldDeclarationSyntax fds:
                loc = fds.Declaration.Variables[0].Identifier.GetLocation();
                break;
            case PropertyDeclarationSyntax pds:
                loc = pds.Identifier.GetLocation();
                break;
            default:
                return;
        }
        
        var diagnostic = Diagnostic.Create(
            InstructionDiagnostics.FieldNoAttribute,
            loc
        );
        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeFieldSize(SyntaxNodeAnalysisContext context) {
        if (context.Node is not FieldDeclarationSyntax && context.Node is not PropertyDeclarationSyntax) {
            return;
        }
        
        SemanticModel semanticModel = context.SemanticModel;
        ISymbol? symbol = semanticModel.GetDeclaredSymbol(context.Node);

        if (symbol is not IFieldSymbol && symbol is not IPropertySymbol) {
            return;
        }
        AttributeData? fieldAttrib = symbol.GetAttributes().FirstOrDefault(x =>
            x.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
            "global::Mercury.Engine.Generators.Instruction.FieldAttribute");

        if (fieldAttrib is null) {
            return;
        }
        
        int value1 = (int)fieldAttrib.ConstructorArguments[0].Value!;
        int value2 = (int)fieldAttrib.ConstructorArguments[1].Value!;
        int desiredSize = Math.Abs(value1 - value2) + 1;

        int variableSize = 0;
        Location? loc;
        if (symbol is IFieldSymbol field) {
            variableSize = GetBitWidth(field.Type);
            loc = field.Locations[0];
        }else if (symbol is IPropertySymbol property) {
            variableSize = GetBitWidth(property.Type);
            loc = property.Locations[0];
        }
        else {
            return;
        }

        if (desiredSize > variableSize) {
            var diagnostic = Diagnostic.Create(
                InstructionDiagnostics.InsufficientFieldSize,
                loc
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeCoverage(SyntaxNodeAnalysisContext context) {
        ClassDeclarationSyntax classDecl = (ClassDeclarationSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;
        INamedTypeSymbol? symbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (symbol is null) {
            return;
        }

        bool hasInstructionAttribute = symbol.GetAttributes()
            .Any(attr => attr.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == 
                         "global::Mercury.Engine.Generators.Instruction.InstructionAttribute");

        if (!hasInstructionAttribute) {
            return;
        }

        uint coverage = 0;
        
        // process formatting
        foreach (AttributeData? attribute in symbol.GetAttributes()) {
            if (attribute is null) {
                continue;
            }
            if (!(attribute.AttributeClass?.Name.StartsWith("FormatExact") ?? true)) {
                continue;
            }
            if (attribute.ConstructorArguments[2].Kind == TypedConstantKind.Array) {
                continue;
            }
            
            int min = (int)attribute.ConstructorArguments[0].Value!;
            int max = (int)attribute.ConstructorArguments[1].Value!;
            if (min > max) {
                (min, max) = (max, min);
            }
            uint mask = (uint)((((long)1 << (max - min + 1)) - 1) << min);
            coverage |= mask;
        }

        // process fields
        foreach (ISymbol? member in symbol.GetMembers()) {
            if (member is not IPropertySymbol && member is not IFieldSymbol) {
                continue;
            }
            AttributeData? fieldAttrib = member.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                "global::Mercury.Engine.Generators.Instruction.FieldAttribute");
            if (fieldAttrib is null) {
                continue;
            }
            int min = (int)fieldAttrib.ConstructorArguments[0].Value!;
            int max = (int)fieldAttrib.ConstructorArguments[1].Value!;
            if (min > max) {
                (min, max) = (max, min);
            }
            uint mask = (uint)(((1 << (max - min + 1)) - 1) << min);
            coverage |= mask;
        }
        if (coverage != uint.MaxValue) {
            var diagnostic = Diagnostic.Create(
                InstructionDiagnostics.FormattingAmbiguity,
                symbol.Locations[0],
                coverage.ToString("X8")
            );
            context.ReportDiagnostic(diagnostic);
        }
    }
    
    private static int GetBitWidth(ITypeSymbol type) {
        return type.SpecialType switch {
            SpecialType.System_Byte => 8,
            SpecialType.System_SByte => 8,
            SpecialType.System_Int16 => 16,
            SpecialType.System_UInt16 => 16,
            SpecialType.System_Int32 => 32,
            SpecialType.System_UInt32 => 32,
            SpecialType.System_Int64 => 64,
            SpecialType.System_UInt64 => 64,
            _ => 0
        };
    }
}