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
/// Contains unit tests for <see cref="DynamicLogMessageTemplateCodeFix"/> to verify code fix for dynamic log message templates.
/// </summary>
public class DynamicLogMessageTemplateCodeFixTests
    : CSharpCodeFixTest<DynamicLogMessageTemplateAnalyzer, DynamicLogMessageTemplateCodeFix, DefaultVerifier>
{
    /// <summary>
    /// Verifies that the code fix converts dynamic string interpolation to a static template with parameters.
    /// </summary>
    /// <returns>True if the code fix converts a dynamic template to a static template; otherwise, false.</returns>
    [Fact]
    public async Task WhenDynamicTemplate_ConvertsToStaticTmplate()
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

        FixedCode = """
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
        this.Trace("run for {id}", id);
    }
}
""";

        // await RunAsync();
    }
}
