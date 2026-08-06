using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mercury.Generators.Instruction;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mercury.Generators.Instruction;

/// <summary>
/// Provides code fixes for <c>MRCY0004</c>: a field or property whose declared type is too small
/// to hold the bit range specified in <c>[Field(bitStart, bitEnd)]</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InsufficientFieldSizeCodeFixProvider)), Shared]
public sealed class InsufficientFieldSizeCodeFixProvider : CodeFixProvider {

    private const string FieldAttributeFqn =
        "global::Mercury.Engine.Generators.Instruction.FieldAttribute";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [InstructionDiagnostics.InsufficientFieldSize.Id];

    public override FixAllProvider? GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        SyntaxNode? root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (root is null) {
            return;
        }

        Diagnostic diagnostic = context.Diagnostics[0];
        SyntaxNode diagnosticNode = root.FindNode(diagnostic.Location.SourceSpan);

        // Walk up to the containing field or property declaration.
        MemberDeclarationSyntax? memberDecl = diagnosticNode
            .AncestorsAndSelf()
            .OfType<MemberDeclarationSyntax>()
            .FirstOrDefault(n => n is FieldDeclarationSyntax or PropertyDeclarationSyntax);

        if (memberDecl is null) {
            return;
        }

        // Resolve how many bits the [Field] attribute covers so we can pick the right type.
        SemanticModel? semanticModel = await context.Document
            .GetSemanticModelAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (semanticModel is null) {
            return;
        }

        ISymbol? symbol = semanticModel.GetDeclaredSymbol(memberDecl, context.CancellationToken);
        if (symbol is null) {
            return;
        }

        AttributeData? fieldAttrib = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == FieldAttributeFqn);

        if (fieldAttrib is null ||
            fieldAttrib.ConstructorArguments.Length < 2 ||
            fieldAttrib.ConstructorArguments[0].Value is not int v1 ||
            fieldAttrib.ConstructorArguments[1].Value is not int v2) {
            return;
        }

        int desiredBits = Math.Abs(v1 - v2) + 1;
        (string unsignedKeyword, string signedKeyword) = PickKeywords(desiredBits);

        // Register both signed and unsigned variants.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Change type to '{unsignedKeyword}' (unsigned, {desiredBits} bits needed)",
                createChangedDocument: ct =>
                    ChangeTypeAsync(context.Document, memberDecl, unsignedKeyword, ct),
                equivalenceKey: $"{nameof(InsufficientFieldSizeCodeFixProvider)}_unsigned"),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Change type to '{signedKeyword}' (signed, {desiredBits} bits needed)",
                createChangedDocument: ct =>
                    ChangeTypeAsync(context.Document, memberDecl, signedKeyword, ct),
                equivalenceKey: $"{nameof(InsufficientFieldSizeCodeFixProvider)}_signed"),
            diagnostic);
    }

    /// <summary>
    /// Returns <c>(unsignedKeyword, signedKeyword)</c> for the smallest integer types that
    /// can hold <paramref name="bits"/> bits.
    /// </summary>
    private static (string Unsigned, string Signed) PickKeywords(int bits) {
        if (bits <= 8)  return ("byte",   "sbyte");
        if (bits <= 16) return ("ushort", "short");
        if (bits <= 32) return ("uint",   "int");
        return              ("ulong",  "long");
    }

    private static async Task<Document> ChangeTypeAsync(
        Document document,
        MemberDeclarationSyntax memberDecl,
        string typeName,
        CancellationToken cancellationToken) {

        TypeSyntax newType = SyntaxFactory
            .PredefinedType(SyntaxFactory.Token(ToSyntaxKind(typeName)))
            .WithTriviaFrom(GetCurrentTypeSyntax(memberDecl));

        SyntaxNode? root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root is null) {
            return document;
        }

        SyntaxNode newRoot = memberDecl switch {
            FieldDeclarationSyntax fds =>
                root.ReplaceNode(fds, fds.WithDeclaration(fds.Declaration.WithType(newType))),

            PropertyDeclarationSyntax pds =>
                root.ReplaceNode(pds, pds.WithType(newType)),

            _ => root
        };

        return document.WithSyntaxRoot(newRoot);
    }

    private static TypeSyntax GetCurrentTypeSyntax(MemberDeclarationSyntax member) =>
        member switch {
            FieldDeclarationSyntax fds    => fds.Declaration.Type,
            PropertyDeclarationSyntax pds => pds.Type,
            _                             => SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.IntKeyword))
        };

    private static SyntaxKind ToSyntaxKind(string keyword) => keyword switch {
        "byte"   => SyntaxKind.ByteKeyword,
        "sbyte"  => SyntaxKind.SByteKeyword,
        "ushort" => SyntaxKind.UShortKeyword,
        "short"  => SyntaxKind.ShortKeyword,
        "uint"   => SyntaxKind.UIntKeyword,
        "int"    => SyntaxKind.IntKeyword,
        "ulong"  => SyntaxKind.ULongKeyword,
        "long"   => SyntaxKind.LongKeyword,
        _        => SyntaxKind.IntKeyword
    };
}
