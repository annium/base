using System.Collections.Generic;
using System.Collections.Immutable;
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
public sealed class ExplicitCallerArgumentAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Metadata names of the caller-info attributes recognised by this analyzer. Hard-coded as strings
    /// because the analyzer assembly targets <c>netstandard2.1</c> and must not take a compile-time
    /// dependency on attribute types (e.g. <c>CallerArgumentExpressionAttribute</c>) that may be absent
    /// from older BCLs. Resolved against each compilation at compilation start (see <see cref="Initialize"/>)
    /// so per-parameter checks become a fast symbol comparison instead of a multi-segment namespace walk.
    /// </summary>
    private static readonly string[] _callerAttributeMetadataNames =
    [
        "System.Runtime.CompilerServices.CallerFilePathAttribute",
        "System.Runtime.CompilerServices.CallerMemberNameAttribute",
        "System.Runtime.CompilerServices.CallerLineNumberAttribute",
        "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute",
    ];

    /// <summary>
    /// Gets the supported diagnostics for this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [Descriptors.Log0002ExplicitCallerArgument];

    /// <summary>
    /// Initializes the analyzer by configuring concurrent execution and registering the operation action.
    /// </summary>
    /// <param name="context">The analysis context to configure.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationCtx =>
        {
            // Resolve the caller-info attribute types once per compilation. When none of them are
            // available (the compilation has no `System.Runtime` reference) there's no work to do.
            var callerAttributeTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            foreach (var metadataName in _callerAttributeMetadataNames)
            {
                var symbol = compilationCtx.Compilation.GetTypeByMetadataName(metadataName);
                if (symbol is not null)
                    callerAttributeTypes.Add(symbol);
            }
            if (callerAttributeTypes.Count == 0)
                return;

            compilationCtx.RegisterOperationAction(
                ctx => AnalyzeOperation(ctx, callerAttributeTypes),
                OperationKind.Invocation
            );
        });
    }

    /// <summary>
    /// Inspects each invocation argument, reporting any explicit value bound to a caller-info parameter.
    /// </summary>
    /// <param name="ctx">The operation analysis context containing the invocation to inspect.</param>
    /// <param name="callerAttributeTypes">Caller-info attribute symbols resolved at compilation start.</param>
    private static void AnalyzeOperation(OperationAnalysisContext ctx, HashSet<INamedTypeSymbol> callerAttributeTypes)
    {
        if (ctx.Operation is not IInvocationOperation invocation)
            return;

        if (!AnalyzerHelpers.IsAnniumLoggingMethod(invocation.TargetMethod))
            return;

        foreach (var arg in invocation.Arguments)
        {
            if (arg.ArgumentKind != ArgumentKind.Explicit)
                continue;

            var parameter = arg.Parameter;
            if (parameter is null)
                continue;

            if (!HasCallerAttribute(parameter, callerAttributeTypes))
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
    /// Returns true if the parameter carries any of the resolved caller-info attribute types.
    /// </summary>
    /// <param name="parameter">The parameter to inspect.</param>
    /// <param name="callerAttributeTypes">The set of caller-info attribute symbols to test against.</param>
    /// <returns><see langword="true"/> if any caller-info attribute is present; otherwise <see langword="false"/>.</returns>
    private static bool HasCallerAttribute(IParameterSymbol parameter, HashSet<INamedTypeSymbol> callerAttributeTypes)
    {
        foreach (var attribute in parameter.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is not null && callerAttributeTypes.Contains(attributeClass))
                return true;
        }

        return false;
    }
}
