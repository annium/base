using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Annium.Analyzers.Logging;

/// <summary>
/// Analyzer that ensures log message templates are constant strings and not interpolated strings.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DynamicLogMessageTemplateAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Collection of logging method names that should have constant message templates.
    /// </summary>
    private static readonly IReadOnlyCollection<string> _methodNames =
    [
        "Debug",
        "Error",
        "Info",
        "Log",
        "Trace",
        "Warn",
    ];

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
    private void AnalyzeOperation(OperationAnalysisContext ctx)
    {
        if (ctx.Operation is not IInvocationOperation invocation)
            return;

        // check assembly
        var method = invocation.TargetMethod;
        if (method.ContainingAssembly.Name != "Annium")
            return;

        // check namespace
        var ns = method.ContainingNamespace;
        if (
            ns.Name != "Logging"
            || ns.ContainingNamespace.Name != "Annium"
            || !ns.ContainingNamespace.ContainingNamespace.IsGlobalNamespace
        )
            return;

        // check method name
        if (!_methodNames.Contains(method.Name))
            return;

        // check method containing type
        var typeName = $"LogSubject{method.Name}Extensions";
        if (method.ContainingType.Name != typeName)
            return;

        // template is 2nd argument, after ILogSubject - ensure it's present
        var args = invocation.Arguments;
        if (args.Length <= 1)
            return;

        // ensure template is not interpolated string
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
