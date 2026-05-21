using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
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
/// e.g. <c>this.Trace($"run for {id}")</c> becomes <c>this.Trace("run for {id}", id)</c>.
/// </summary>
/// <remarks>
/// <b>Operation-vs-Syntax argument-index impedance:</b> the analyzer fires when <c>IInvocationOperation.Arguments[1]</c>
/// (the template) is an interpolated string. At the operation layer <c>args[0]</c> is the extension-method receiver
/// (the <c>this ILogSubject</c>) and <c>args[1]</c> is the template. At the syntax layer the receiver appears as
/// the member-access target of the invocation (<c>this.Trace(...)</c>) — NOT as a syntax argument — so the template
/// appears at <c>InvocationExpressionSyntax.ArgumentList.Arguments[0]</c> in the typical positional shape. To stay
/// robust against named-argument shapes (<c>this.Trace(message: $"...")</c>) this code fix locates the
/// interpolated string by scanning the syntax arguments, not by position alone.
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class DynamicLogMessageTemplateCodeFix : CodeFixProvider
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

        // Locate the interpolated-string argument by scanning syntax arguments instead of indexing
        // positionally — this handles the named-argument shape `this.Trace(message: $"...")` as well as
        // the typical positional one.
        var arguments = invocation.ArgumentList.Arguments;
        var interpolatedIndex = -1;
        InterpolatedStringExpressionSyntax? interpolated = null;
        for (var i = 0; i < arguments.Count; i++)
        {
            if (arguments[i].Expression is InterpolatedStringExpressionSyntax candidate)
            {
                interpolatedIndex = i;
                interpolated = candidate;
                break;
            }
        }
        if (interpolated is null)
            return;

        // Refuse the auto-fix when any interpolation carries an alignment or format clause
        // (e.g. `{x,10:F2}`). Converting these to structured-log placeholders silently drops the
        // formatting directive; the developer should resolve manually rather than have semantics
        // changed under them.
        if (HasAlignmentOrFormat(interpolated))
            return;

        var diagnostic = context.Diagnostics[0];

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to static log template",
                createChangedDocument: ct =>
                    ConvertAsync(context.Document, invocation, interpolated, interpolatedIndex, ct),
                equivalenceKey: nameof(DynamicLogMessageTemplateCodeFix)
            ),
            diagnostic
        );
    }

    /// <summary>
    /// Returns <see langword="true"/> if any interpolation inside <paramref name="interpolated"/> carries
    /// a non-null <see cref="InterpolationSyntax.AlignmentClause"/> or
    /// <see cref="InterpolationSyntax.FormatClause"/>. Such formatting cannot be preserved by the
    /// structured-template rewrite, so the code fix refuses these inputs rather than dropping the
    /// directive silently.
    /// </summary>
    /// <param name="interpolated">The interpolated string to inspect.</param>
    /// <returns><see langword="true"/> when any interpolation has alignment or format; otherwise <see langword="false"/>.</returns>
    private static bool HasAlignmentOrFormat(InterpolatedStringExpressionSyntax interpolated)
    {
        foreach (var part in interpolated.Contents)
        {
            if (
                part is InterpolationSyntax interpolation
                && (interpolation.AlignmentClause is not null || interpolation.FormatClause is not null)
            )
                return true;
        }
        return false;
    }

    /// <summary>
    /// Builds a new invocation with the interpolated argument replaced by a literal template
    /// and each interpolation captured as an additional argument.
    /// </summary>
    /// <param name="document">Document containing the invocation.</param>
    /// <param name="invocation">Original invocation syntax.</param>
    /// <param name="interpolated">The interpolated-string argument to be converted.</param>
    /// <param name="interpolatedIndex">Index of the interpolated argument in the original argument list.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated document.</returns>
    private static async Task<Document> ConvertAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        InterpolatedStringExpressionSyntax interpolated,
        int interpolatedIndex,
        CancellationToken ct
    )
    {
        // Pre-pass: count baseName occurrences so we can suffix EVERY repeat from 1 (not from 2 on the
        // first occurrence's repeat). `$"{x} and {x}"` becomes `"{x1} and {x2}", x, x` — both slots are
        // suffixed and named consistently, instead of the asymmetric `{x}, {x2}` shape.
        var baseNameTotals = new Dictionary<string, int>();
        foreach (var part in interpolated.Contents)
        {
            if (part is InterpolationSyntax interpolation)
            {
                var baseName = DerivePlaceholderName(interpolation.Expression);
                baseNameTotals[baseName] = baseNameTotals.TryGetValue(baseName, out var c) ? c + 1 : 1;
            }
        }

        var template = new StringBuilder();
        var captured = new List<ExpressionSyntax>();
        var baseNameSeen = new Dictionary<string, int>();

        foreach (var part in interpolated.Contents)
        {
            switch (part)
            {
                case InterpolatedStringTextSyntax text:
                    template.Append(text.TextToken.ValueText);
                    break;
                case InterpolationSyntax interpolation:
                    var baseName = DerivePlaceholderName(interpolation.Expression);
                    var occurrence = baseNameSeen.TryGetValue(baseName, out var seen) ? seen + 1 : 1;
                    baseNameSeen[baseName] = occurrence;
                    // Suffix from 1 only when the base name appears more than once in the template.
                    var name = baseNameTotals[baseName] > 1 ? $"{baseName}{occurrence}" : baseName;

                    template.Append('{').Append(name).Append('}');
                    captured.Add(interpolation.Expression.WithoutTrivia());
                    break;
            }
        }

        var literal = SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(template.ToString())
        );

        // Preserve every argument other than the interpolated one in its original position, then append
        // the captured interpolation values. Locating the interpolated argument by index (rather than
        // by `Skip(1)`) handles named-argument shapes correctly.
        var existing = invocation.ArgumentList.Arguments;
        var nodes = new List<SyntaxNodeOrToken>();
        for (var i = 0; i < existing.Count; i++)
        {
            if (i == interpolatedIndex)
                nodes.Add(SyntaxFactory.Argument(literal));
            else
                nodes.Add(existing[i].WithoutTrivia());

            if (i < existing.Count - 1)
                nodes.Add(SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space));
        }

        foreach (var expr in captured)
        {
            nodes.Add(SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space));
            nodes.Add(SyntaxFactory.Argument(expr));
        }

        var newArgList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(nodes));
        var newInvocation = invocation.WithArgumentList(newArgList);

        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;
        var newRoot = root.ReplaceNode(invocation, newInvocation);

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
