using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Annium.Analyzers.Logging;

/// <summary>
/// Analyzer that ensures log message templates are constant strings and not interpolated strings.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DynamicLogMessageTemplateAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the supported diagnostics for this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [Descriptors.Log0001DynamicLogMessageTemplate];

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
    /// Analyzes an operation to check if it's a logging method call with a non-constant message template.
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

        if (args[1].Value is not IInterpolatedStringOperation)
            return;

        ctx.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: Descriptors.Log0001DynamicLogMessageTemplate,
                location: invocation.Syntax.GetLocation()
            )
        );
    }
}
