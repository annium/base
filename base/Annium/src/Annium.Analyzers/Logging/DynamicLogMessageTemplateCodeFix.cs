using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Annium.Analyzers.Logging;

/// <summary>
/// Converts an interpolated-string log call into the static-template / named-args form expected by Annium logging,
/// e.g. <c>logger.Trace($"run for {id}")</c> becomes <c>logger.Trace("run for {id}", id)</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class DynamicLogMessageTemplateCodeFix : CodeFixProvider
{
    /// <summary>
    /// Diagnostic IDs handled by this code fix.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [Descriptors.Log0001DynamicLogMessageTemplate.Id];

    /// <summary>
    /// Returns the batch fix-all provider so the fix can be applied across documents/projects.
    /// </summary>
    /// <returns>The batch fix-all provider.</returns>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    /// Registers the conversion code action for the diagnostic.
    /// </summary>
    /// <param name="context">Context supplied by the IDE.</param>
    /// <returns>A task that completes once the code fix has been registered.</returns>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var node = root.FindNode(context.Span);
        var invocation = node as InvocationExpressionSyntax ?? node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null)
            return;

        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count == 0)
            return;

        if (arguments[0].Expression is not InterpolatedStringExpressionSyntax interpolated)
            return;

        var diagnostic = context.Diagnostics[0];

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to static log template",
                createChangedDocument: ct => ConvertAsync(context.Document, invocation, interpolated, ct),
                equivalenceKey: nameof(DynamicLogMessageTemplateCodeFix)
            ),
            diagnostic
        );
    }

    /// <summary>
    /// Builds a new invocation with the interpolated first argument replaced by a literal template
    /// and each interpolation captured as an additional argument.
    /// </summary>
    /// <param name="document">Document containing the invocation.</param>
    /// <param name="invocation">Original invocation syntax.</param>
    /// <param name="interpolated">The interpolated-string argument to be converted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated document.</returns>
    private static async Task<Document> ConvertAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        InterpolatedStringExpressionSyntax interpolated,
        CancellationToken ct
    )
    {
        var template = new StringBuilder();
        var captured = new List<ExpressionSyntax>();
        var nameCounts = new Dictionary<string, int>();

        foreach (var part in interpolated.Contents)
        {
            switch (part)
            {
                case InterpolatedStringTextSyntax text:
                    template.Append(text.TextToken.ValueText);
                    break;
                case InterpolationSyntax interpolation:
                    var baseName = DerivePlaceholderName(interpolation.Expression);
                    var name = baseName;
                    if (nameCounts.TryGetValue(baseName, out var count))
                    {
                        nameCounts[baseName] = count + 1;
                        name = $"{baseName}{count + 1}";
                    }
                    else
                    {
                        nameCounts[baseName] = 1;
                    }

                    template.Append('{').Append(name).Append('}');
                    captured.Add(interpolation.Expression.WithoutTrivia());
                    break;
            }
        }

        var literal = SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(template.ToString())
        );

        var existing = invocation.ArgumentList.Arguments;
        var nodes = new List<SyntaxNodeOrToken> { SyntaxFactory.Argument(literal) };

        foreach (var arg in existing.Skip(1))
        {
            nodes.Add(SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space));
            nodes.Add(arg);
        }

        foreach (var expr in captured)
        {
            nodes.Add(SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space));
            nodes.Add(SyntaxFactory.Argument(expr));
        }

        var newArgList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(nodes));
        var newInvocation = invocation.WithArgumentList(newArgList);

        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        var newRoot = root!.ReplaceNode(invocation, newInvocation);

        return document.WithSyntaxRoot(newRoot);
    }

    /// <summary>
    /// Picks a placeholder name for an interpolation expression — uses the identifier itself
    /// (or the trailing member of a member-access chain), and falls back to <c>value</c>.
    /// </summary>
    /// <param name="expression">Interpolation expression.</param>
    /// <returns>The placeholder name.</returns>
    private static string DerivePlaceholderName(ExpressionSyntax expression) =>
        expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax member => member.Name.Identifier.Text,
            _ => "value",
        };
}
