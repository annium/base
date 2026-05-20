using System.Threading.Tasks;
using Annium.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Annium.Analyzers.Tests.Logging;

/// <summary>
/// Verifies <see cref="ExplicitCallerArgumentCodeFix"/> rewrites caller-info-overriding log calls.
/// </summary>
public sealed class ExplicitCallerArgumentCodeFixTests
    : CSharpCodeFixTest<ExplicitCallerArgumentAnalyzer, ExplicitCallerArgumentCodeFix, DefaultVerifier>
{
    /// <summary>
    /// <c>this.Error(ex, "msg")</c> is rewritten to <c>this.Error("msg: {exception}", ex)</c> so that the
    /// message reaches the templated overload instead of being lost in the file-path slot.
    /// </summary>
    [Fact]
    public async Task ExceptionOverloadWithStringSecondArg_ConvertsToTemplated()
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

        FixedCode = """
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
        this.Error("HandleClosed failed: {exception}", ex);
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
    /// A trailing positional caller-info argument on a non-Exception overload is just removed,
    /// so the compiler-injected default takes over again.
    /// </summary>
    [Fact]
    public async Task TrailingPositionalCallerArgument_RemovesArgument()
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

    public void Run(int id)
    {
        this.Trace<int>("run for {id}", id);
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
    /// A named caller-info argument is removed, leaving the compiler default in place.
    /// </summary>
    [Fact]
    public async Task NamedCallerArgument_RemovesArgument()
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

    public void Run()
    {
        this.Trace("hello");
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

    /// <summary>
    /// When any argument in the invocation carries a name-colon, <c>TryConvertExceptionShape</c>
    /// bails out and the fix falls back to simply removing the explicit caller-info argument.
    /// The error message is NOT rewritten to <c>"msg: {exception}"</c> form.
    /// </summary>
    [Fact]
    public async Task ExceptionShapeWithNamedCallerArg_FallsBackToRemoveArgument()
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
        this.Error(ex, file: "src.cs");
    }
}
""";

        FixedCode = """
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
        this.Error(ex);
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0002ExplicitCallerArgument.Id, DiagnosticSeverity.Warning)
                .WithMessage("Argument bound to 'file' overrides a compiler-injected caller-info value")
                .WithSpan(17, 24, 17, 38)
                .WithArguments("file")
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// When the invocation has more than two positional arguments, <c>TryConvertExceptionShape</c>
    /// bails out on the argument-count check and the fix falls back to simply removing the
    /// explicit caller-info argument rather than performing the Exception-shape rewrite.
    /// </summary>
    [Fact]
    public async Task NonExceptionShapeWithThreeArgs_FallsBackToRemoveArgument()
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
        this.Error<int>("template {id}", id, "src.cs");
    }
}
""";

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

    public void Run(int id)
    {
        this.Error<int>("template {id}", id);
    }
}
""";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Log0002ExplicitCallerArgument.Id, DiagnosticSeverity.Warning)
                .WithMessage("Argument bound to 'file' overrides a compiler-injected caller-info value")
                .WithSpan(16, 46, 16, 54)
                .WithArguments("file")
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }
}
