using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Annium.Analyzers.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DynamicLogMessageTemplateAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Descriptors.Log0001DynamicLogMessageTemplate);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // context.RegisterCodeBlockAction(AnalyzeCodeBlock);
        context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
        // context.RegisterSymbolAction(action: AnalyzeNamedType, symbolKinds: SymbolKind.NamedType);
    }

    private void AnalyzeOperation(OperationAnalysisContext ctx)
    {
        if (ctx.Operation is not IInvocationOperation invocation)
            return;

        var method = invocation.TargetMethod;
        if (method.ContainingAssembly.Name != "Annium")
            return;

        var ns = method.ContainingNamespace;
        if (
            ns.Name != "Logging"
            || ns.ContainingNamespace.Name != "Annium"
            || !ns.ContainingNamespace.ContainingNamespace.IsGlobalNamespace
        )
            return;

        Console.WriteLine("OK");
    }

    // private void AnalyzeNamedType(SymbolAnalysisContext ctx)
    // {
    //     var symbol = (INamedTypeSymbol)ctx.Symbol;
    //     if (symbol.TypeKind != TypeKind.Class)
    //         return;
    //
    //     if (symbol.Name.EndsWith("Exception"))
    //         return;
    //
    //     if (!IsException(symbol, ctx.Compilation.GetTypeByMetadataName(typeof(Exception).FullName!)!))
    //         return;
    //
    //     ctx.ReportDiagnostic(
    //         Diagnostic.Create(
    //             descriptor: Descriptors.Pg0001ExceptionNameFormat,
    //             location: symbol.Locations.First(),
    //             messageArgs: symbol.Name
    //         )
    //     );
    // }
    //
    // private bool IsException(INamedTypeSymbol classSymbol, INamedTypeSymbol exceptionTypeSymbol)
    // {
    //     if (classSymbol.Equals(exceptionTypeSymbol, SymbolEqualityComparer.Default))
    //         return true;
    //
    //     var baseClass = classSymbol.BaseType;
    //     return baseClass is not null && IsException(baseClass, exceptionTypeSymbol);
    // }
}
