using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Annium.Analyzers.Logging;

/// <summary>
/// Fixes <see cref="Descriptors.Log0002ExplicitCallerArgument"/> diagnostics. For the special
/// <c>this.Error(ex, "msg")</c> shape (where the developer mistakenly bound to the
/// <c>Error(Exception, [CallerFilePath] string file, ...)</c> overload) the call is rewritten as
/// <c>this.Error("msg: {exception}", ex)</c> so the message is preserved through the templated overload.
/// For every other shape the explicitly-passed caller-info argument is simply removed so the
/// compiler-injected default takes over again.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ExplicitCallerArgumentCodeFix : CodeFixProvider
{
    /// <summary>
    /// Diagnostic IDs handled by this code fix.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [Descriptors.Log0002ExplicitCallerArgument.Id];

    /// <summary>
    /// Returns the batch fix-all provider so the fix can be applied across documents/projects.
    /// </summary>
    /// <returns>The batch fix-all provider.</returns>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    /// Registers the fix action for the diagnostic.
    /// </summary>
    /// <param name="context">Context supplied by the IDE.</param>
    /// <returns>A task that completes once the code fix has been registered.</returns>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var node = root.FindNode(context.Span);
        var argument = node as ArgumentSyntax ?? node.FirstAncestorOrSelf<ArgumentSyntax>();
        if (argument is null)
            return;

        var invocation = argument.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null)
            return;

        var diagnostic = context.Diagnostics[0];

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Fix logging caller-info argument",
                createChangedDocument: ct => FixAsync(context.Document, invocation, argument, ct),
                equivalenceKey: nameof(ExplicitCallerArgumentCodeFix)
            ),
            diagnostic
        );
    }

    /// <summary>
    /// Applies either the templated rewrite or the simple-remove fix to the document.
    /// </summary>
    /// <param name="document">Document containing the invocation.</param>
    /// <param name="invocation">Original invocation syntax.</param>
    /// <param name="flagged">The argument flagged by the diagnostic.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated document.</returns>
    private static async Task<Document> FixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        ArgumentSyntax flagged,
        CancellationToken ct
    )
    {
        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);

        InvocationExpressionSyntax newInvocation =
            (semanticModel is not null ? TryConvertExceptionShape(invocation, semanticModel, ct) : null)
            ?? RemoveArgument(invocation, flagged);

        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        var newRoot = root!.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }

    /// <summary>
    /// Removes the flagged argument from the invocation's argument list.
    /// </summary>
    /// <param name="invocation">Original invocation syntax.</param>
    /// <param name="flagged">The argument to drop.</param>
    /// <returns>An invocation with the argument removed.</returns>
    private static InvocationExpressionSyntax RemoveArgument(
        InvocationExpressionSyntax invocation,
        ArgumentSyntax flagged
    )
    {
        var newArgs = invocation.ArgumentList.Arguments.Remove(flagged);
        return invocation.WithArgumentList(invocation.ArgumentList.WithArguments(newArgs));
    }

    /// <summary>
    /// Attempts to rewrite the <c>Error(Exception, string)</c> misuse pattern into the templated
    /// <c>Error("msg: {exception}", ex)</c> form. Returns <see langword="null"/> when the invocation
    /// shape doesn't match (so the caller falls back to the simple-remove fix).
    /// </summary>
    /// <param name="invocation">Original invocation syntax.</param>
    /// <param name="semanticModel">Semantic model for symbol/type lookups.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rewritten invocation, or <see langword="null"/> if the shape didn't match.</returns>
    private static InvocationExpressionSyntax? TryConvertExceptionShape(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken ct
    )
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count != 2)
            return null;

        // Bail out if either arg uses a name colon — we only handle the unambiguous positional shape.
        if (args[0].NameColon is not null || args[1].NameColon is not null)
            return null;

        // Target method must take Exception as its first non-this parameter.
        if (semanticModel.GetSymbolInfo(invocation, ct).Symbol is not IMethodSymbol method)
            return null;

        if (method.Parameters.Length == 0 || !IsExceptionType(method.Parameters[0].Type))
            return null;

        // First arg must actually be an Exception expression (defensive — the binding guarantees it).
        var firstType = semanticModel.GetTypeInfo(args[0].Expression, ct).Type;
        if (firstType is null || !IsExceptionType(firstType))
            return null;

        // Second arg must be a literal string — that's what the developer thought was the message.
        if (
            args[1].Expression is not LiteralExpressionSyntax literal
            || !literal.IsKind(SyntaxKind.StringLiteralExpression)
        )
            return null;

        var newMessage = SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(literal.Token.ValueText + ": {exception}")
        );

        var newArgList = SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList(
                new[] { SyntaxFactory.Argument(newMessage), SyntaxFactory.Argument(args[0].Expression.WithoutTrivia()) }
            )
        );

        // Strip explicit type arguments — the new overload (Error<T1>(string, T1, ...)) infers T1 from the call site.
        var expression = invocation.Expression switch
        {
            MemberAccessExpressionSyntax { Name: GenericNameSyntax gen } mae => mae.WithName(
                SyntaxFactory.IdentifierName(gen.Identifier)
            ),
            GenericNameSyntax gen => SyntaxFactory.IdentifierName(gen.Identifier),
            _ => invocation.Expression,
        };

        return invocation.WithExpression(expression).WithArgumentList(newArgList);
    }

    /// <summary>
    /// Returns true when <paramref name="type"/> is <c>System.Exception</c> or any subclass.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> when the type is or derives from <c>System.Exception</c>.</returns>
    private static bool IsExceptionType(ITypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.Name != "Exception")
                continue;

            var ns = current.ContainingNamespace;
            if (ns is null || ns.Name != "System" || ns.ContainingNamespace?.IsGlobalNamespace != true)
                continue;

            return true;
        }

        return false;
    }
}
