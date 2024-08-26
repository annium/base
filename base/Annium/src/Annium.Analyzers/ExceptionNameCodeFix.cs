using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Annium.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ExceptionNameCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [Descriptors.Pg0001ExceptionNameFormat.Id];

    public override FixAllProvider GetFixAllProvider()
    {
        return null!;
    }

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
