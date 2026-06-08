using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Annium.Analyzers.Logging;

/// <summary>
/// Analyzer that flags constant log message templates with unbalanced <c>{</c> / <c>}</c> braces.
/// </summary>
/// <remarks>
/// Mirrors the brace grammar of the runtime template processor (<c>Annium.Logging.Shared.Internal.Helper</c>):
/// braces may nest, but an unmatched closing <c>}</c> or an unclosed opening <c>{</c> corrupts the
/// rendered message and the structured-data dictionary at runtime. Catching it at compile time turns a
/// silent malformed-output bug into a build warning. Only constant string templates are analyzed —
/// non-constant templates are the domain of <see cref="DynamicLogMessageTemplateAnalyzer"/> (LOG0001).
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MalformedLogMessageTemplateAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the supported diagnostics for this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [Descriptors.Log0003MalformedLogMessageTemplate];

    /// <summary>
    /// Initializes the analyzer by configuring concurrent execution and registering the operation action.
    /// </summary>
    /// <param name="context">The analysis context to configure.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
    }

    /// <summary>
    /// Analyzes an invocation to check whether an Annium logging call carries a constant message
    /// template with unbalanced braces.
    /// </summary>
    /// <param name="ctx">The operation analysis context containing the operation to analyze.</param>
    private static void AnalyzeOperation(OperationAnalysisContext ctx)
    {
        if (ctx.Operation is not IInvocationOperation invocation)
            return;

        if (!AnalyzerHelpers.IsAnniumLoggingMethod(invocation.TargetMethod))
            return;

        // Operation API includes the extension receiver as args[0]; the template is args[1].
        var args = invocation.Arguments;
        if (args.Length <= 1)
            return;

        // Only constant string templates are analyzed. Overloads where args[1] is not a string
        // (e.g. Error(Exception)) are skipped; non-constant templates are LOG0001's domain.
        var templateValue = args[1].Value;
        if (templateValue.Type?.SpecialType != SpecialType.System_String)
            return;

        if (!templateValue.ConstantValue.HasValue || templateValue.ConstantValue.Value is not string template)
            return;

        if (!IsMalformedTemplate(template))
            return;

        ctx.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: Descriptors.Log0003MalformedLogMessageTemplate,
                location: invocation.Syntax.GetLocation()
            )
        );
    }

    /// <summary>
    /// Returns <see langword="true"/> when the template's braces are unbalanced — an unmatched closing
    /// <c>}</c> (closing brace with no open one) or an unclosed opening <c>{</c> at end of string.
    /// Balanced nesting (e.g. <c>{a{b}c}</c>) is well-formed, matching the runtime depth-based parser.
    /// </summary>
    /// <param name="template">The constant message template to validate.</param>
    /// <returns><see langword="true"/> if the braces are unbalanced; otherwise <see langword="false"/>.</returns>
    private static bool IsMalformedTemplate(string template)
    {
        var depth = 0;
        foreach (var ch in template)
            switch (ch)
            {
                case '{':
                    depth++;
                    break;
                case '}':
                    if (depth == 0)
                        return true; // unmatched closing brace
                    depth--;
                    break;
            }

        return depth != 0; // unclosed opening brace
    }
}
