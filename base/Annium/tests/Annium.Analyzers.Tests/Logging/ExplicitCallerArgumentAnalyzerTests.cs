using System.Threading.Tasks;
using Annium.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Annium.Analyzers.Tests.Logging;

/// <summary>
/// Contains unit tests for <see cref="ExplicitCallerArgumentAnalyzer"/> covering caller-info parameter overrides.
/// </summary>
public sealed class ExplicitCallerArgumentAnalyzerTests
    : CSharpAnalyzerTest<ExplicitCallerArgumentAnalyzer, DefaultVerifier>
{
    /// <summary>
    /// Calls that omit the caller-info parameters should pass through silently.
    /// </summary>
    [Fact]
    public async Task NoExplicitCallerArguments_Ignored()
    {
        ReferenceAssemblies = LoggingAnalyzerTestHelpers.BuildReferenceAssemblies();

        TestCode = """
using System;
using Annium.Logging;

namespace Test;

public class Sample : ILogSubject
{
    public ILogger Logger { get; }

    public Sample(ILogger logger)
    {
        Logger = logger;
    }

    public void RunMessage()
    {
        this.Trace("hello");
    }

    public void RunMessageWithArg(int id)
    {
        this.Trace("hello {id}", id);
    }

    public void RunException(Exception ex)
    {
        this.Error(ex);
    }
}
""";

        ExpectedDiagnostics.Clear();

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// <c>this.Error(ex, "msg")</c> resolves to <c>Error(Exception, [CallerFilePath] string file, ...)</c>;
    /// the literal "msg" silently overrides the file-path slot and must be reported.
    /// </summary>
    [Fact]
    public async Task ExceptionOverloadWithStringSecondArg_Reports()
    {
        ReferenceAssemblies = LoggingAnalyzerTestHelpers.BuildReferenceAssemblies();

        TestCode = """
using System;
using Annium.Logging;

namespace Test;

public class Sample : ILogSubject
{
    public ILogger Logger { get; }

    public Sample(ILogger logger)
    {
        Logger = logger;
    }

    public void Run(Exception ex)
    {
        this.Error(ex, "HandleClosed failed");
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0002ExplicitCallerArgument.Id, DiagnosticSeverity.Warning)
                .WithMessage("Argument bound to 'file' overrides a compiler-injected caller-info value")
                .WithSpan(17, 24, 17, 45)
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Trailing positional caller arguments (e.g. <c>this.Trace("msg", id, "")</c> with the final string going
    /// to the <c>file</c> slot) must be reported.
    /// </summary>
    [Fact]
    public async Task TrailingPositionalCallerArgument_Reports()
    {
        ReferenceAssemblies = LoggingAnalyzerTestHelpers.BuildReferenceAssemblies();

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

    public void Run(int id)
    {
        this.Trace<int>("run for {id}", id, "src.cs");
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0002ExplicitCallerArgument.Id, DiagnosticSeverity.Warning)
                .WithMessage("Argument bound to 'file' overrides a compiler-injected caller-info value")
                .WithSpan(16, 45, 16, 53)
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Named caller-info arguments must be reported even though they are syntactically unambiguous.
    /// </summary>
    [Fact]
    public async Task NamedCallerArgument_Reports()
    {
        ReferenceAssemblies = LoggingAnalyzerTestHelpers.BuildReferenceAssemblies();

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

    public void Run()
    {
        this.Trace("hello", file: "src.cs");
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0002ExplicitCallerArgument.Id, DiagnosticSeverity.Warning)
                .WithMessage("Argument bound to 'file' overrides a compiler-injected caller-info value")
                .WithSpan(16, 29, 16, 43)
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }
}
