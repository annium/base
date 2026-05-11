using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Annium.Analyzers;

/// <summary>
/// Shared helpers consumed by the analyzers and code-fixes in this assembly. Centralizes the
/// load-bearing string literals that identify Annium logging APIs (assembly + namespace + extension
/// class pattern) and the exception-hierarchy walk used by both the naming analyzer and the
/// caller-info code fix.
/// </summary>
internal static class AnalyzerHelpers
{
    /// <summary>
    /// Annium assembly name. Load-bearing for the logging analyzers' gate — see
    /// <see cref="IsAnniumLoggingMethod"/>.
    /// </summary>
    internal const string AnniumAssembly = "Annium";

    /// <summary>
    /// Annium logging namespace name (last segment). The logging gate expects this to live at
    /// <c>Annium.Logging</c> directly under the global namespace.
    /// </summary>
    internal const string AnniumLoggingNamespace = "Logging";

    /// <summary>
    /// Suffix used to build the per-verb logging extension class name pattern
    /// (<c>LogSubject{Verb}Extensions</c>). Load-bearing for the logging gate.
    /// </summary>
    internal const string LogSubjectExtensionsClassSuffix = "Extensions";

    /// <summary>
    /// Logging extension method names whose calls are analyzed by both logging analyzers. A new verb
    /// MUST be added here (single source of truth) and to the matching <c>LogSubject{Verb}Extensions</c>
    /// class in <c>Annium.Logging</c>.
    /// </summary>
    internal static readonly IReadOnlyCollection<string> LogMethodNames =
    [
        "Debug",
        "Error",
        "Info",
        "Log",
        "Trace",
        "Warn",
    ];

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="method"/> is a call to one of the Annium
    /// logging extension methods (<c>LogSubject{Verb}Extensions</c> in namespace <c>Annium.Logging</c>
    /// of the <c>Annium</c> assembly). Used by both logging analyzers as the assembly + namespace +
    /// method-name + containing-type gate.
    /// </summary>
    /// <param name="method">The invoked method symbol.</param>
    /// <returns><see langword="true"/> if the method is an Annium logging extension; otherwise <see langword="false"/>.</returns>
    internal static bool IsAnniumLoggingMethod(IMethodSymbol method)
    {
        if (method.ContainingAssembly?.Name != AnniumAssembly)
            return false;

        var ns = method.ContainingNamespace;
        if (
            ns is null
            || ns.Name != AnniumLoggingNamespace
            || ns.ContainingNamespace?.Name != AnniumAssembly
            || ns.ContainingNamespace.ContainingNamespace?.IsGlobalNamespace != true
        )
            return false;

        if (!LogMethodNames.Contains(method.Name))
            return false;

        var expectedTypeName = $"LogSubject{method.Name}{LogSubjectExtensionsClassSuffix}";
        return method.ContainingType?.Name == expectedTypeName;
    }

    /// <summary>
    /// Walks <paramref name="type"/>'s base-type chain and returns <see langword="true"/> when any link
    /// equals <paramref name="exceptionType"/> under <see cref="SymbolEqualityComparer.Default"/>.
    /// Single source of truth for "is this an exception type" semantics shared between
    /// <c>ExceptionNameAnalyzer</c> (resolves the symbol at compilation start) and
    /// <c>ExplicitCallerArgumentCodeFix</c> (resolves it on demand via the code-fix's semantic model).
    /// </summary>
    /// <param name="type">The type symbol to test.</param>
    /// <param name="exceptionType">The resolved <c>System.Exception</c> symbol.</param>
    /// <returns><see langword="true"/> when <paramref name="type"/> is or derives from <paramref name="exceptionType"/>.</returns>
    internal static bool InheritsFromException(ITypeSymbol type, INamedTypeSymbol exceptionType)
    {
        for (ITypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, exceptionType))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves <c>System.Exception</c> in the given <paramref name="compilation"/>. Returns
    /// <see langword="null"/> when the compilation does not reference <c>mscorlib</c> /
    /// <c>System.Runtime</c> — callers MUST tolerate that case (analyzer: skip the rule, code-fix:
    /// fall back to the non-exception path) rather than dereferencing the result.
    /// </summary>
    /// <param name="compilation">The compilation to resolve against.</param>
    /// <returns>The resolved <c>System.Exception</c> symbol, or <see langword="null"/> when it can't be found.</returns>
    internal static INamedTypeSymbol? ResolveExceptionType(Compilation compilation) =>
        compilation.GetTypeByMetadataName(typeof(Exception).FullName!);
}
