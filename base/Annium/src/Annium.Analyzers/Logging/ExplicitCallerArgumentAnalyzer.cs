using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Annium.Analyzers.Logging;

/// <summary>
/// Analyzer that flags explicit values passed to parameters of Annium logging extension methods that carry
/// caller-info attributes. Such values silently override the compiler-injected file/member/line metadata —
/// usually because the caller bound to a different overload than they expected (e.g. <c>this.Error(ex, "msg")</c>
/// resolves to <c>Error(Exception, [CallerFilePath] string file, ...)</c> with <c>"msg"</c> stuffed into the file slot).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExplicitCallerArgumentAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Logging extension method names whose caller-info parameters must remain compiler-injected.
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
    /// Caller-info attribute simple names recognised by this analyzer.
    /// </summary>
    private static readonly IReadOnlyCollection<string> _callerAttributeNames =
    [
        "CallerFilePathAttribute",
        "CallerMemberNameAttribute",
        "CallerLineNumberAttribute",
        "CallerArgumentExpressionAttribute",
    ];

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [Descriptors.Log0002ExplicitCallerArgument];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
    }

    /// <summary>
    /// Inspects each invocation argument, reporting any explicit value bound to a caller-info parameter.
    /// </summary>
    /// <param name="ctx">The operation analysis context containing the invocation to inspect.</param>
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

        foreach (var arg in invocation.Arguments)
        {
            if (arg.ArgumentKind != ArgumentKind.Explicit)
                continue;

            var parameter = arg.Parameter;
            if (parameter is null)
                continue;

            if (!HasCallerAttribute(parameter))
                continue;

            ctx.ReportDiagnostic(
                Diagnostic.Create(
                    descriptor: Descriptors.Log0002ExplicitCallerArgument,
                    location: arg.Syntax.GetLocation(),
                    messageArgs: parameter.Name
                )
            );
        }
    }

    /// <summary>
    /// Returns true if the parameter carries any caller-info attribute from <c>System.Runtime.CompilerServices</c>.
    /// </summary>
    /// <param name="parameter">The parameter to inspect.</param>
    /// <returns><see langword="true"/> if any caller-info attribute is present; otherwise <see langword="false"/>.</returns>
    private static bool HasCallerAttribute(IParameterSymbol parameter)
    {
        foreach (var attribute in parameter.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null)
                continue;

            var attributeNs = attributeClass.ContainingNamespace;
            if (
                attributeNs is null
                || attributeNs.Name != "CompilerServices"
                || attributeNs.ContainingNamespace?.Name != "Runtime"
                || attributeNs.ContainingNamespace.ContainingNamespace?.Name != "System"
                || attributeNs.ContainingNamespace.ContainingNamespace.ContainingNamespace?.IsGlobalNamespace != true
            )
                continue;

            if (_callerAttributeNames.Contains(attributeClass.Name))
                return true;
        }

        return false;
    }
}
