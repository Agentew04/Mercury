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
/// Provides code fixes for <c>MRCY0001</c>: instruction classes that do not implement <c>IInstruction</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ImplementInterfaceCodeFixProvider)), Shared]
public sealed class ImplementInterfaceCodeFixProvider : CodeFixProvider {

    private const string IInstructionFqn = "Mercury.Engine.Common.IInstruction";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [InstructionDiagnostics.ImplementInterface.Id];

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
        ClassDeclarationSyntax? classDecl = diagnosticNode
            .AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDecl is null) {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Implement '{IInstructionFqn}'",
                createChangedDocument: ct => AddInterfaceAsync(context.Document, classDecl, ct),
                equivalenceKey: nameof(ImplementInterfaceCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> AddInterfaceAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        CancellationToken cancellationToken) {

        // Build the simple type name. A using directive for the namespace is
        // added separately so the editor can resolve it.
        SimpleNameSyntax interfaceName = SyntaxFactory.IdentifierName("IInstruction");
        SimpleBaseTypeSyntax baseType = SyntaxFactory.SimpleBaseType(interfaceName);

        BaseListSyntax newBaseList;
        if (classDecl.BaseList is null) {
            // No base list yet — create one with just IInstruction.
            newBaseList = SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType))
                .WithLeadingTrivia(SyntaxFactory.Space);
        } else {
            // Append IInstruction to the existing base list.
            newBaseList = classDecl.BaseList.AddTypes(baseType);
        }

        ClassDeclarationSyntax newClassDecl = classDecl.WithBaseList(newBaseList);

        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) {
            return document;
        }

        SyntaxNode newRoot = root.ReplaceNode(classDecl, newClassDecl);

        // Add a using directive if the namespace isn't already imported.
        if (newRoot is CompilationUnitSyntax compilationUnit) {
            bool hasUsing = compilationUnit.Usings.Any(u =>
                u.Name?.ToString() == "Mercury.Engine.Common");

            if (!hasUsing) {
                UsingDirectiveSyntax usingDirective = SyntaxFactory
                    .UsingDirective(SyntaxFactory.ParseName("Mercury.Engine.Common"))
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

                newRoot = compilationUnit.AddUsings(usingDirective);
            }
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
