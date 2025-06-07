using System.IO;
using System.Threading.Tasks;
using Annium.Analyzers.Logging;
using Annium.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Annium.Analyzers.Tests.Logging;

/// <summary>
/// Contains unit tests for <see cref="DynamicLogMessageTemplateAnalyzer"/> to verify log message template analysis.
/// </summary>
public class DynamicLogMessageTemplateAnalyzerTests
    : CSharpAnalyzerTest<DynamicLogMessageTemplateAnalyzer, DefaultVerifier>
{
    /// <summary>
    /// Verifies that the analyzer ignores constant log message templates.
    /// </summary>
    /// <returns>True if the analyzer ignores constant templates; otherwise, false.</returns>
    [Fact]
    public async Task ConstantTemplate_Ignores()
    {
        ReferenceAssemblies = new ReferenceAssemblies(
            ReferenceAssemblies.NetStandard.NetStandard21.TargetFramework,
            ReferenceAssemblies.NetStandard.NetStandard21.ReferenceAssemblyPackage,
            Directory.GetCurrentDirectory()
        ).AddAssemblies([typeof(ILogSubject).Assembly.GetName().Name!]);

        TestCode = """
using Annium.Logging;

namespace Test;

public class Sample : ILogSubject
{
    public ILogger Logger { get; }

    public Sample(ILogger logger)
    {
        Logger = logger;
    }

    public void Setup(int id)
    {
        this.Trace<int>("run for {id}", id, "");
    }
}
""";

        ExpectedDiagnostics.Clear();

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that the analyzer shows a warning for dynamic log message templates.
    /// </summary>
    /// <returns>True if the analyzer shows a warning for dynamic templates; otherwise, false.</returns>
    [Fact]
    public async Task DynamicTemplate_ShowsWarning()
    {
        ReferenceAssemblies = new ReferenceAssemblies(
            ReferenceAssemblies.NetStandard.NetStandard21.TargetFramework,
            ReferenceAssemblies.NetStandard.NetStandard21.ReferenceAssemblyPackage,
            Directory.GetCurrentDirectory()
        ).AddAssemblies([typeof(ILogSubject).Assembly.GetName().Name!]);

        TestCode = """
using Annium.Logging;

namespace Test;

public class Sample : ILogSubject
{
    public ILogger Logger { get; }

    public Sample(ILogger logger)
    {
        Logger = logger;
    }

    public void Setup(int id)
    {
        this.Trace($"run for {id}");
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0001DynamicLogMessageTemplate.Id, DiagnosticSeverity.Warning)
                .WithMessage("Call message template is non-constant")
                .WithSpan(16, 9, 16, 36)
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }
}
