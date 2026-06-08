using System.Threading.Tasks;
using Annium.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Annium.Analyzers.Tests.Logging;

/// <summary>
/// Contains unit tests for <see cref="MalformedLogMessageTemplateAnalyzer"/> verifying unbalanced-brace
/// detection in constant log message templates.
/// </summary>
public sealed class MalformedLogMessageTemplateAnalyzerTests
    : CSharpAnalyzerTest<MalformedLogMessageTemplateAnalyzer, DefaultVerifier>
{
    /// <summary>
    /// Initializes the test with the shared logging-analyzer reference assemblies.
    /// </summary>
    public MalformedLogMessageTemplateAnalyzerTests()
    {
        ReferenceAssemblies = LoggingAnalyzerTestHelpers.BuildReferenceAssemblies();
    }

    /// <summary>
    /// Verifies that a well-formed (balanced-brace) template produces no diagnostic.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BalancedTemplate_Ignores()
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

    public void Setup()
    {
        this.Trace("plain {value}");
    }
}
""";

        ExpectedDiagnostics.Clear();

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that an unmatched closing brace produces a warning.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task UnmatchedCloseBrace_ShowsWarning()
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

    public void Setup()
    {
        this.Trace("oops}");
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0003MalformedLogMessageTemplate.Id, DiagnosticSeverity.Warning)
                .WithMessage(LoggingAnalyzerTestHelpers.MalformedTemplateMessage)
                .WithSpan(16, 9, 16, 28)
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that an unclosed opening brace produces a warning.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task UnclosedOpenBrace_ShowsWarning()
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

    public void Setup()
    {
        this.Trace("oops{");
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0003MalformedLogMessageTemplate.Id, DiagnosticSeverity.Warning)
                .WithMessage(LoggingAnalyzerTestHelpers.MalformedTemplateMessage)
                .WithSpan(16, 9, 16, 28)
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }
}
