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
