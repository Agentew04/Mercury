using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Mercury.Generators.Instruction;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class InstructionAnalyzer : DiagnosticAnalyzer {
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        InstructionDiagnostics.ImplementInterface,
        InstructionDiagnostics.UsePartial,
        InstructionDiagnostics.FieldNoAttribute,
        InstructionDiagnostics.InsufficientFieldSize,
        InstructionDiagnostics.FormattingAmbiguity,
        InstructionDiagnostics.InvalidAssemblySpecifier,
        InstructionDiagnostics.InvalidAssemblyFormat,
        InstructionDiagnostics.AssemblyFormatUnknownField
    ];
    
    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInterfaceUsage, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePartialClass, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeFieldAttribute, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeFieldSize, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeCoverage, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeAssemblyFormat, SyntaxKind.ClassDeclaration);
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

    // -------------------------------------------------------------------------
    // Assembly format string analysis
    // -------------------------------------------------------------------------

    private static void AnalyzeAssemblyFormat(SyntaxNodeAnalysisContext context) {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol) {
            return;
        }

        // Only run on [Instruction] classes
        bool hasInstruction = classSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                      == "global::Mercury.Engine.Generators.Instruction.InstructionAttribute");
        if (!hasInstruction) {
            return;
        }

        // Find the [AssemblyFormat("...")] attribute on the class declaration
        AttributeSyntax? assemblyFormatAttrSyntax = classDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => {
                SymbolInfo si = semanticModel.GetSymbolInfo(a);
                ISymbol? sym = si.Symbol ?? si.CandidateSymbols.FirstOrDefault();
                return sym?.ContainingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                       == "global::Mercury.Engine.Generators.Instruction.AssemblyFormatAttribute";
            });

        if (assemblyFormatAttrSyntax is null) {
            return;
        }

        // Extract the string literal argument
        if (assemblyFormatAttrSyntax.ArgumentList?.Arguments.Count is not > 0) {
            return;
        }

        ExpressionSyntax argExpr = assemblyFormatAttrSyntax.ArgumentList!.Arguments[0].Expression;
        if (argExpr is not LiteralExpressionSyntax literal ||
            !literal.IsKind(SyntaxKind.StringLiteralExpression)) {
            return;
        }

        string format = literal.Token.ValueText; // unescaped content
        // The opening '"' is at literal.Token.SpanStart, content starts at SpanStart+1
        int tokenContentStart = literal.Token.SpanStart + 1;
        SyntaxTree syntaxTree = context.Node.SyntaxTree;

        // Collect all [AssemblyFormatter] methods visible in the compilation
        List<(string Specifier, string Namespace)> knownFormatters =
            CollectAssemblyFormatters(context.SemanticModel.Compilation);

        // Collect all [Field]-decorated member names in this class
        HashSet<string> fieldMembers = new(StringComparer.Ordinal);
        foreach (ISymbol member in classSymbol.GetMembers()) {
            if (member is not IPropertySymbol && member is not IFieldSymbol) continue;
            bool hasField = member.GetAttributes().Any(a =>
                a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                == "global::Mercury.Engine.Generators.Instruction.FieldAttribute");
            if (hasField) {
                fieldMembers.Add(member.Name);
            }
        }

        // Determine the instruction's namespace for formatter resolution
        string instructionNamespace = classSymbol.ContainingNamespace
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "");

        // Walk the format string
        int i = 0;
        while (i < format.Length) {
            if (format[i] == '{') {
                // Escaped '{{'
                if (i + 1 < format.Length && format[i + 1] == '{') {
                    i += 2;
                    continue;
                }

                int placeholderStart = i; // position of '{' in content string
                int closeIndex = format.IndexOf('}', i);
                if (closeIndex == -1) {
                    // Unclosed brace — MRCY0007
                    // Highlight from '{' to end of string
                    Location loc = Location.Create(syntaxTree,
                        TextSpan.FromBounds(
                            tokenContentStart + placeholderStart,
                            tokenContentStart + format.Length));
                    context.ReportDiagnostic(Diagnostic.Create(
                        InstructionDiagnostics.InvalidAssemblyFormat, loc));
                    return;
                }

                string content = format.Substring(i + 1, closeIndex - i - 1);
                int colonIndex = content.IndexOf(':');

                if (colonIndex != -1) {
                    string varName  = content.Substring(0, colonIndex).Trim();
                    string specifier = content.Substring(colonIndex + 1).Trim();

                    // Location of the specifier token inside the string
                    // e.g.  {Rt:reg}  — specifier 'reg' starts at i+1+colonIndex+1
                    int specifierOffset = i + 1 + colonIndex + 1;
                    Location specLoc = Location.Create(syntaxTree,
                        TextSpan.FromBounds(
                            tokenContentStart + specifierOffset,
                            tokenContentStart + specifierOffset + specifier.Length));

                    // Location of the varName token inside the string
                    int varOffset = i + 1;
                    Location varLoc = Location.Create(syntaxTree,
                        TextSpan.FromBounds(
                            tokenContentStart + varOffset,
                            tokenContentStart + varOffset + varName.Length));

                    // MRCY0008 — varName must be a [Field] member
                    if (!string.IsNullOrEmpty(varName) && !fieldMembers.Contains(varName)) {
                        context.ReportDiagnostic(Diagnostic.Create(
                            InstructionDiagnostics.AssemblyFormatUnknownField, varLoc, varName));
                    }

                    // MRCY0006 — specifier must be a known formatter or standard .NET format
                    if (!string.IsNullOrEmpty(specifier)) {
                        bool isCustom = FindFormatterInList(specifier, instructionNamespace, knownFormatters);
                        if (!isCustom && !IsValidStandardFormatSpecifier(specifier)) {
                            context.ReportDiagnostic(Diagnostic.Create(
                                InstructionDiagnostics.InvalidAssemblySpecifier, specLoc, specifier));
                        }
                    }
                }

                i = closeIndex + 1;
            } else if (format[i] == '}') {
                // Skip escaped '}}'
                i += (i + 1 < format.Length && format[i + 1] == '}') ? 2 : 1;
            } else {
                i++;
            }
        }
    }

    /// <summary>Collects all static methods decorated with [AssemblyFormatter(specifier)] across the compilation.</summary>
    private static List<(string Specifier, string Namespace)> CollectAssemblyFormatters(
        Compilation compilation) {

        const string formatterFqn =
            "global::Mercury.Engine.Generators.Instruction.AssemblyFormatterAttribute";

        var result = new List<(string, string)>();
        foreach (INamedTypeSymbol type in GetAllTypes(compilation.GlobalNamespace)) {
            foreach (IMethodSymbol method in type.GetMembers().OfType<IMethodSymbol>()) {
                if (!method.IsStatic) continue;
                foreach (AttributeData attr in method.GetAttributes()) {
                    if (attr.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        != formatterFqn) continue;
                    if (attr.ConstructorArguments.Length == 0) continue;
                    string specifier = attr.ConstructorArguments[0].Value as string ?? "";
                    string ns = method.ContainingNamespace
                        .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        .Replace("global::", "");
                    result.Add((specifier, ns));
                }
            }
        }
        return result;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns) {
        foreach (INamedTypeSymbol type in ns.GetTypeMembers()) {
            yield return type;
        }
        foreach (INamespaceSymbol nested in ns.GetNamespaceMembers()) {
            foreach (INamedTypeSymbol type in GetAllTypes(nested)) {
                yield return type;
            }
        }
    }

    /// <summary>Nearest-namespace-first lookup matching the generator's FindFormatter logic.</summary>
    private static bool FindFormatterInList(
        string specifier,
        string instructionNamespace,
        List<(string Specifier, string Namespace)> formatters) {

        string ns = instructionNamespace;
        while (!string.IsNullOrEmpty(ns)) {
            if (formatters.Any(f => f.Specifier == specifier && f.Namespace == ns)) {
                return true;
            }
            int lastDot = ns.LastIndexOf('.');
            if (lastDot == -1) break;
            ns = ns.Substring(0, lastDot);
        }
        return formatters.Any(f => f.Specifier == specifier);
    }

    /// <summary>Returns true for standard .NET format specifiers: one letter from [XxDdBbGgFfNn] optionally followed by digits.</summary>
    private static bool IsValidStandardFormatSpecifier(string specifier) {
        if (string.IsNullOrEmpty(specifier)) return true;
        char first = specifier[0];
        if (first == 'X' || first == 'x' || first == 'D' || first == 'd' ||
            first == 'B' || first == 'b' || first == 'G' || first == 'g' ||
            first == 'F' || first == 'f' || first == 'N' || first == 'n') {
            for (int j = 1; j < specifier.Length; j++) {
                if (!char.IsDigit(specifier[j])) return false;
            }
            return true;
        }
        return false;
    }
}