using System.Threading.Tasks;
using Annium.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Annium.Analyzers.Tests.Logging;

/// <summary>
/// Verifies <see cref="DynamicLogMessageTemplateCodeFix"/> rewrites interpolated log calls
/// into the static-template / named-args form.
/// </summary>
public sealed class DynamicLogMessageTemplateCodeFixTests
    : CSharpCodeFixTest<DynamicLogMessageTemplateAnalyzer, DynamicLogMessageTemplateCodeFix, DefaultVerifier>
{
    /// <summary>
    /// Initializes the test with the shared logging-analyzer reference assemblies.
    /// </summary>
    public DynamicLogMessageTemplateCodeFixTests()
    {
        ReferenceAssemblies = LoggingAnalyzerTestHelpers.BuildReferenceAssemblies();
    }

    /// <summary>
    /// Single interpolation: <c>$"run for {id}"</c> becomes <c>"run for {id}", id</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenDynamicTemplate_ConvertsToStaticTemplate()
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
        this.Trace($"run for {id}");
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0001DynamicLogMessageTemplate.Id, DiagnosticSeverity.Warning)
                .WithMessage(LoggingAnalyzerTestHelpers.DynamicTemplateMessage)
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

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// An interpolation with an alignment clause (e.g. <c>{x,10}</c>) cannot be safely rewritten to a
    /// structured-log placeholder because the alignment directive would be silently dropped.
    /// The analyzer still flags the call, but the code fix must not register any action —
    /// so <c>FixedCode</c> equals <c>TestCode</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenInterpolationHasAlignment_NoCodeFixRegistered()
    {
        var code = """
using Annium.Logging;

namespace Test;

public class Sample : ILogSubject
{
    public ILogger Logger { get; }

    public Sample(ILogger logger)
    {
        Logger = logger;
    }

    public void Setup(int x)
    {
        this.Trace($"value {x,10}");
    }
}
""";

        TestCode = code;

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0001DynamicLogMessageTemplate.Id, DiagnosticSeverity.Warning)
                .WithMessage(LoggingAnalyzerTestHelpers.DynamicTemplateMessage)
                .WithSpan(16, 9, 16, 36)
        );

        // The fix is refused for aligned interpolations, so the document must not change.
        FixedCode = code;

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// An interpolation with a format clause (e.g. <c>{x:F2}</c>) cannot be safely rewritten because the
    /// format specifier would be silently dropped.  The analyzer still flags the call, but the code fix
    /// must not register any action — so <c>FixedCode</c> equals <c>TestCode</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenInterpolationHasFormatClause_NoCodeFixRegistered()
    {
        var code = """
using Annium.Logging;

namespace Test;

public class Sample : ILogSubject
{
    public ILogger Logger { get; }

    public Sample(ILogger logger)
    {
        Logger = logger;
    }

    public void Setup(double x)
    {
        this.Trace($"value {x:F2}");
    }
}
""";

        TestCode = code;

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0001DynamicLogMessageTemplate.Id, DiagnosticSeverity.Warning)
                .WithMessage(LoggingAnalyzerTestHelpers.DynamicTemplateMessage)
                .WithSpan(16, 9, 16, 36)
        );

        // The fix is refused for format-clause interpolations, so the document must not change.
        FixedCode = code;

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// When the same identifier appears more than once inside an interpolated string, the fix must suffix
    /// EVERY occurrence starting from 1 — producing <c>{x1}</c> and <c>{x2}</c> — not the asymmetric
    /// <c>{x}</c> and <c>{x2}</c> shape that would result from suffixing only from the second occurrence.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenDuplicateIdentifierInTemplate_BothPlaceholdersSuffixed()
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

    public void Setup(int x)
    {
        this.Trace($"{x} and {x}");
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0001DynamicLogMessageTemplate.Id, DiagnosticSeverity.Warning)
                .WithMessage(LoggingAnalyzerTestHelpers.DynamicTemplateMessage)
                .WithSpan(16, 9, 16, 35)
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

    public void Setup(int x)
    {
        this.Trace("{x1} and {x2}", x, x);
    }
}
""";

        await RunAsync(TestContext.Current.CancellationToken);
    }
}
