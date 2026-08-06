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
/// Provides code fixes for <c>MRCY0002</c>: instruction classes that are missing the <c>partial</c> modifier.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsePartialCodeFixProvider)), Shared]
public sealed class UsePartialCodeFixProvider : CodeFixProvider {

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [InstructionDiagnostics.UsePartial.Id];

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
        SyntaxToken token = root.FindToken(diagnostic.Location.SourceSpan.Start);
        ClassDeclarationSyntax? classDecl = token.Parent?.AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDecl is null) {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add 'partial' modifier",
                createChangedDocument: ct => AddPartialModifierAsync(context.Document, classDecl, ct),
                equivalenceKey: nameof(UsePartialCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> AddPartialModifierAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        CancellationToken cancellationToken) {

        // Insert 'partial' before the 'class' keyword, preserving trivia.
        SyntaxToken partialToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword)
            .WithTrailingTrivia(SyntaxFactory.Space);

        SyntaxTokenList newModifiers = classDecl.Modifiers.Add(partialToken);
        ClassDeclarationSyntax newClassDecl = classDecl.WithModifiers(newModifiers);

        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        SyntaxNode newRoot = root!.ReplaceNode(classDecl, newClassDecl);
        return document.WithSyntaxRoot(newRoot);
    }
}
