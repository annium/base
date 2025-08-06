using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Annium.Analyzers;

/// <summary>
/// Analyzer that enforces exception class naming convention by ensuring that exception classes end with "Exception".
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExceptionNameAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the supported diagnostics for this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [Descriptors.Pg0001ExceptionNameFormat];

    /// <summary>
    /// Initializes the analyzer by configuring concurrent execution and registering the symbol action.
    /// </summary>
    /// <param name="context">The analysis context to configure.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );

        context.RegisterSymbolAction(action: AnalyzeNamedType, symbolKinds: SymbolKind.NamedType);
    }

    /// <summary>
    /// Analyzes a named type symbol to check if it's an exception class that doesn't follow the naming convention.
    /// </summary>
    /// <param name="ctx">The symbol analysis context containing the type to analyze.</param>
    private void AnalyzeNamedType(SymbolAnalysisContext ctx)
    {
        var symbol = (INamedTypeSymbol)ctx.Symbol;
        if (symbol.TypeKind != TypeKind.Class)
            return;

        if (symbol.Name.EndsWith("Exception"))
            return;

        if (!IsException(symbol, ctx.Compilation.GetTypeByMetadataName(typeof(Exception).FullName!)!))
            return;

        ctx.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: Descriptors.Pg0001ExceptionNameFormat,
                location: symbol.Locations.First(),
                messageArgs: symbol.Name
            )
        );
    }

    /// <summary>
    /// Determines if a type symbol represents an exception class by checking if it is or inherits from System.Exception.
    /// </summary>
    /// <param name="classSymbol">The type symbol to check.</param>
    /// <param name="exceptionTypeSymbol">The System.Exception type symbol.</param>
    /// <returns>True if the type is or inherits from System.Exception, false otherwise.</returns>
    private static bool IsException(INamedTypeSymbol classSymbol, INamedTypeSymbol exceptionTypeSymbol)
    {
        if (classSymbol.Equals(exceptionTypeSymbol, SymbolEqualityComparer.Default))
            return true;

        var baseClass = classSymbol.BaseType;
        return baseClass is not null && IsException(baseClass, exceptionTypeSymbol);
    }
}
