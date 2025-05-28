using System.IO;
using System.Threading.Tasks;
using Annium.Analyzers.Logging;
using Annium.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Annium.Analyzers.Tests.Logging;

public class DynamicLogMessageTemplateAnalyzerTests
    : CSharpAnalyzerTest<DynamicLogMessageTemplateAnalyzer, DefaultVerifier>
{
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
