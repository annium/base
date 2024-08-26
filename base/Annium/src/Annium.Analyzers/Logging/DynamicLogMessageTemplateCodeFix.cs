using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Annium.Analyzers.Logging;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class DynamicLogMessageTemplateCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        [Descriptors.Log0001DynamicLogMessageTemplate.Id];

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

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
