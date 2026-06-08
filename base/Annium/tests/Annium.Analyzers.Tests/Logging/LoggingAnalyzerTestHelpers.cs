using System.IO;
using Annium.Logging;
using Microsoft.CodeAnalysis.Testing;

namespace Annium.Analyzers.Tests.Logging;

/// <summary>
/// Shared helpers for analyzer / code-fix tests in the Logging area.
/// </summary>
internal static class LoggingAnalyzerTestHelpers
{
    /// <summary>
    /// Expected diagnostic message for LOG0001 (dynamic, non-constant log message template).
    /// </summary>
    public const string DynamicTemplateMessage = "Call message template is non-constant";

    /// <summary>
    /// Expected diagnostic message for LOG0003 (malformed, unbalanced-brace log message template).
    /// </summary>
    public const string MalformedTemplateMessage = "Log message template has unbalanced '{' / '}' braces";

    /// <summary>
    /// Expected diagnostic message for LOG0002 when the explicit argument is bound to the 'file' caller-info value.
    /// </summary>
    public const string ExplicitCallerFileMessage =
        "Argument bound to 'file' overrides a compiler-injected caller-info value";

    /// <summary>
    /// Builds the reference assemblies set used by every Logging analyzer / code-fix test fixture.
    /// </summary>
    /// <returns>A configured <see cref="ReferenceAssemblies"/> instance.</returns>
    public static ReferenceAssemblies BuildReferenceAssemblies() =>
        new ReferenceAssemblies(
            ReferenceAssemblies.NetStandard.NetStandard21.TargetFramework,
            ReferenceAssemblies.NetStandard.NetStandard21.ReferenceAssemblyPackage,
            Directory.GetCurrentDirectory()
        ).AddAssemblies([typeof(ILogSubject).Assembly.GetName().Name!]);
}
