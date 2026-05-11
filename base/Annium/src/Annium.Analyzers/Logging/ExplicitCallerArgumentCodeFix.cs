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
public sealed class ExplicitCallerArgumentCodeFix : CodeFixProvider
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
        if (root is null)
            return document;
        var newRoot = root.ReplaceNode(invocation, newInvocation);
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
    /// <remarks>
    /// The first user-visible parameter is located via <see cref="GetFirstUserParameter"/> to handle both
    /// reduced (<c>method.IsExtensionMethod</c> with <c>this</c> stripped) and unreduced extension-method
    /// symbol shapes — indexing <c>Parameters[0]</c> blindly would mistakenly check the <c>this ILogSubject</c>
    /// receiver type on the unreduced form and cause the rewrite to silently drop the exception value.
    /// </remarks>
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

        if (semanticModel.GetSymbolInfo(invocation, ct).Symbol is not IMethodSymbol method)
            return null;

        // Use a single source of truth (AnalyzerHelpers.InheritsFromException) for "is this an
        // exception type" so the analyzer-side and code-fix-side checks cannot drift apart.
        // If the compilation does not reference System.Exception we cannot prove the shape, so
        // gracefully fall back to the simple-remove path.
        var exceptionType = AnalyzerHelpers.ResolveExceptionType(semanticModel.Compilation);
        if (exceptionType is null)
            return null;

        var firstUserParameter = GetFirstUserParameter(method);
        if (
            firstUserParameter is null
            || !AnalyzerHelpers.InheritsFromException(firstUserParameter.Type, exceptionType)
        )
            return null;

        // First arg must actually be an Exception expression (defensive — the binding guarantees it).
        var firstType = semanticModel.GetTypeInfo(args[0].Expression, ct).Type;
        if (firstType is null || !AnalyzerHelpers.InheritsFromException(firstType, exceptionType))
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
    /// Returns the first user-visible parameter of <paramref name="method"/>, normalising over the
    /// reduced (extension-receiver stripped) and unreduced shapes Roslyn may surface for an extension
    /// method invocation. For non-extension methods the first parameter is returned directly.
    /// </summary>
    /// <param name="method">The target method symbol.</param>
    /// <returns>The first user-visible parameter, or <see langword="null"/> when none exists.</returns>
    private static IParameterSymbol? GetFirstUserParameter(IMethodSymbol method)
    {
        // method.IsExtensionMethod is true for both the reduced and unreduced forms. In the reduced
        // form Parameters[0] is the first user-visible parameter (the `this` receiver has been
        // stripped). In the unreduced form Parameters[0] IS the `this` receiver, so the first
        // user-visible parameter is Parameters[1]. method.ReducedFrom is non-null only on reduced
        // symbols.
        if (method.IsExtensionMethod && method.ReducedFrom is null)
        {
            // Unreduced extension form — skip the `this` receiver.
            return method.Parameters.Length > 1 ? method.Parameters[1] : null;
        }

        return method.Parameters.Length > 0 ? method.Parameters[0] : null;
    }
}
