using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Annium.Analyzers;

/// <summary>
/// Analyzer that enforces exception class naming convention by ensuring that exception classes end with "Exception".
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionNameAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the supported diagnostics for this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [Descriptors.An0001ExceptionNameFormat];

    /// <summary>
    /// Initializes the analyzer by configuring concurrent execution and registering the symbol action
    /// inside a compilation-start closure that resolves <c>System.Exception</c> once per compilation.
    /// </summary>
    /// <remarks>
    /// The well-known <c>System.Exception</c> type is resolved via
    /// <see cref="AnalyzerHelpers.ResolveExceptionType"/> at compilation start. When the compilation
    /// does not reference <c>mscorlib</c> / <c>System.Runtime</c> (source-only builds, certain
    /// test fixtures, embedded runtimes) the lookup returns <see langword="null"/> and the symbol
    /// action is simply not registered for the run — far safer than letting a per-symbol
    /// <see cref="Compilation.GetTypeByMetadataName(string)"/> deference crash the analyzer host
    /// (AD0001) and silently disable the rule.
    /// </remarks>
    /// <param name="context">The analysis context to configure.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationCtx =>
        {
            var exceptionType = AnalyzerHelpers.ResolveExceptionType(compilationCtx.Compilation);
            if (exceptionType is null)
                return;

            compilationCtx.RegisterSymbolAction(ctx => AnalyzeNamedType(ctx, exceptionType), SymbolKind.NamedType);
        });
    }

    /// <summary>
    /// Analyzes a named type symbol to check if it's an exception class that doesn't follow the naming convention.
    /// </summary>
    /// <param name="ctx">The symbol analysis context containing the type to analyze.</param>
    /// <param name="exceptionType">The resolved <c>System.Exception</c> symbol, captured from the compilation-start closure.</param>
    private static void AnalyzeNamedType(SymbolAnalysisContext ctx, INamedTypeSymbol exceptionType)
    {
        var symbol = (INamedTypeSymbol)ctx.Symbol;
        if (symbol.TypeKind != TypeKind.Class)
            return;

        if (symbol.Name.EndsWith("Exception"))
            return;

        if (!AnalyzerHelpers.InheritsFromException(symbol, exceptionType))
            return;

        ctx.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: Descriptors.An0001ExceptionNameFormat,
                location: symbol.Locations.First(),
                messageArgs: symbol.Name
            )
        );
    }
}
