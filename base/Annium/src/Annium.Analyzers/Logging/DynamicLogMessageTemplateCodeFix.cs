using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Annium.Analyzers.Logging;

/// <summary>
/// Provides a code fix for dynamic log message templates by suggesting renaming to a static template.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class DynamicLogMessageTemplateCodeFix : CodeFixProvider
{
    /// <summary>
    /// Gets the diagnostic IDs that this code fix provider can handle.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [Descriptors.Log0001DynamicLogMessageTemplate.Id];

    /// <summary>
    /// Gets the provider used to fix all occurrences of a diagnostic in a document/project/solution.
    /// </summary>
    /// <returns>A <see cref="FixAllProvider"/> to fix all occurrences of a diagnostic.</returns>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    /// Registers code fixes for the specified diagnostics.
    /// </summary>
    /// <param name="context">A context for code fix registration.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        // var node = root?.FindNode(context.Span);
        //
        // if (node is not InvocationExpressionSyntax invocation)
        //     return;
        //
        // var document = context.Document;
        // var solution = document.Project.Solution;
        //
        // var documentSemanticModel = await document.GetSemanticModelAsync(context.CancellationToken);
        // if (documentSemanticModel is null)
        //     return;
        //
        // var classModel = documentSemanticModel.GetDeclaredSymbol(invocation, context.CancellationToken);
        // if (classModel is null)
        //     return;
        //
        // var suggestedName = $"{invocation.Identifier.Text}Exception";
        // SyntaxFactory.

        // context.RegisterCodeFix(
        //     CodeAction.Create(
        //         equivalenceKey: "rename",
        //         title: $"Rename to {suggestedName}",
        //         createChangedSolution: async cancellationToken =>
        //             await Renamer.RenameSymbolAsync(
        //                 solution,
        //                 classModel,
        //                 new SymbolRenameOptions(RenameInStrings: true, RenameInComments: true, RenameFile: true),
        //                 suggestedName,
        //                 cancellationToken
        //             )
        //     ),
        //     context.Diagnostics
        // );
    }
}
