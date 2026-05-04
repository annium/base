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
/// Verifies <see cref="ExplicitCallerArgumentCodeFix"/> rewrites caller-info-overriding log calls.
/// </summary>
public class ExplicitCallerArgumentCodeFixTests
    : CSharpCodeFixTest<ExplicitCallerArgumentAnalyzer, ExplicitCallerArgumentCodeFix, DefaultVerifier>
{
    /// <summary>
    /// Builds the reference assemblies set used for every test in this fixture.
    /// </summary>
    /// <returns>A configured <see cref="ReferenceAssemblies"/> instance.</returns>
    private static ReferenceAssemblies BuildReferenceAssemblies() =>
        new ReferenceAssemblies(
            ReferenceAssemblies.NetStandard.NetStandard21.TargetFramework,
            ReferenceAssemblies.NetStandard.NetStandard21.ReferenceAssemblyPackage,
            Directory.GetCurrentDirectory()
        ).AddAssemblies([typeof(ILogSubject).Assembly.GetName().Name!]);

    /// <summary>
    /// <c>this.Error(ex, "msg")</c> is rewritten to <c>this.Error("msg: {exception}", ex)</c> so that the
    /// message reaches the templated overload instead of being lost in the file-path slot.
    /// </summary>
    [Fact]
    public async Task ExceptionOverloadWithStringSecondArg_ConvertsToTemplated()
    {
        ReferenceAssemblies = BuildReferenceAssemblies();

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
        ReferenceAssemblies = BuildReferenceAssemblies();

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
        ReferenceAssemblies = BuildReferenceAssemblies();

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
}
