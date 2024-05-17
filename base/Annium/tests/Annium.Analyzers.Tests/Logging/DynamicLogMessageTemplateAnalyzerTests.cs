using System.IO;
using System.Threading.Tasks;
using Annium.Analyzers.Logging;
using Annium.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace Annium.Analyzers.Tests.Logging;

public class DynamicLogMessageTemplateAnalyzerTests
    : CSharpAnalyzerTest<DynamicLogMessageTemplateAnalyzer, XUnitVerifier>
{
    [Fact]
    public async Task ConstantTemplate_Ignores()
    {
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

        ReferenceAssemblies = new ReferenceAssemblies(
            ReferenceAssemblies.NetStandard.NetStandard21.TargetFramework,
            ReferenceAssemblies.NetStandard.NetStandard21.ReferenceAssemblyPackage,
            Directory.GetCurrentDirectory()
        ).AddAssemblies(
            [
                // typeof(ILogSubject).Assembly.Location,
                typeof(ILogSubject).Assembly.GetName().Name!,
            ]
        );
        // ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21.AddAssemblies(
        //     [typeof(ILogSubject).Assembly.Location, typeof(ILogSubject).Assembly.GetName().Name!,]
        // );
        ExpectedDiagnostics.Clear();

        await RunAsync();
    }

    [Fact]
    public async Task DynamicTemplate_ShowsWarning()
    {
        TestCode = """
using Annium.Logging;

namespace Test

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
                .WithMessage("CustomError class name should end with Exception")
                .WithSpan(1, 14, 1, 25)
        );

        await RunAsync();
    }
}
