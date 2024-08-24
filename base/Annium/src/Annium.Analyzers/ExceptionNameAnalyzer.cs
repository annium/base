using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Annium.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExceptionNameAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [Descriptors.Pg0001ExceptionNameFormat];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );

        context.RegisterSymbolAction(action: AnalyzeNamedType, symbolKinds: SymbolKind.NamedType);
    }

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

    private static bool IsException(INamedTypeSymbol classSymbol, INamedTypeSymbol exceptionTypeSymbol)
    {
        if (classSymbol.Equals(exceptionTypeSymbol, SymbolEqualityComparer.Default))
            return true;

        var baseClass = classSymbol.BaseType;
        return baseClass is not null && IsException(baseClass, exceptionTypeSymbol);
    }
}
