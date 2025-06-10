using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Annium.Analyzers;

/// <summary>
/// Provides a code fix for exception classes that don't follow the naming convention by appending "Exception" to their names.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ExceptionNameCodeFix : CodeFixProvider
{
    /// <summary>
    /// Gets the diagnostic IDs that this code fix provider can fix.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [Descriptors.Pg0001ExceptionNameFormat.Id];

    /// <summary>
    /// Gets the fix all provider for this code fix provider.
    /// </summary>
    /// <returns>Returns null as this code fix provider doesn't support fix all operations.</returns>
    public override FixAllProvider GetFixAllProvider()
    {
        return null!;
    }

    /// <summary>
    /// Registers code fixes for the given context.
    /// </summary>
    /// <param name="context">The code fix context containing the diagnostic and document to fix.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var node = root?.FindNode(context.Span);

        if (node is not ClassDeclarationSyntax classDeclaration)
            return;

        var document = context.Document;
        var solution = document.Project.Solution;

        var documentSemanticModel = await document.GetSemanticModelAsync(context.CancellationToken);
        if (documentSemanticModel is null)
            return;

        var classModel = documentSemanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
        if (classModel is null)
            return;

        var suggestedName = $"{classDeclaration.Identifier.Text}Exception";

        context.RegisterCodeFix(
            CodeAction.Create(
                equivalenceKey: "rename",
                title: $"Rename to {suggestedName}",
                createChangedSolution: async cancellationToken =>
                    await Renamer.RenameSymbolAsync(
                        solution,
                        classModel,
                        new SymbolRenameOptions(RenameInStrings: true, RenameInComments: true, RenameFile: true),
                        suggestedName,
                        cancellationToken
                    )
            ),
            context.Diagnostics
        );
    }
}
