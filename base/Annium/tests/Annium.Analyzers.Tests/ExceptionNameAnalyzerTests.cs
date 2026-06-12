using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Annium.Analyzers.Tests;

/// <summary>
/// Contains unit tests for <see cref="ExceptionNameAnalyzer"/> to verify exception naming conventions.
/// </summary>
public sealed class ExceptionNameAnalyzerTests : CSharpAnalyzerTest<ExceptionNameAnalyzer, DefaultVerifier>
{
    /// <summary>
    /// Verifies that the analyzer ignores correctly named exception classes.
    /// </summary>
    /// <returns>True if the analyzer ignores correct exception names; otherwise, false.</returns>
    [Fact]
    public async Task WhenCorrectName_Ignores()
    {
        TestState.Sources.Add(("CustomException.cs", "public class CustomException : System.Exception { }"));

        ExpectedDiagnostics.Clear();

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that the analyzer shows a warning for incorrectly named exception classes.
    /// </summary>
    /// <returns>True if the analyzer shows a warning for inconsistent exception names; otherwise, false.</returns>
    [Fact]
    public async Task WhenInconsistentName_ShowsWarning()
    {
        TestState.Sources.Add(("CustomError.cs", "public class CustomError : System.Exception { }"));

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.An0001ExceptionNameFormat.Id, DiagnosticSeverity.Warning)
                .WithMessage("CustomError class name should end with Exception")
                .WithSpan("CustomError.cs", 1, 14, 1, 25)
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that the analyzer flags an indirect subclass of <see cref="System.Exception"/>
    /// whose name does not end with "Exception".
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task MultilevelInheritance_WrongName_ShowsWarning()
    {
        TestState.Sources.Add(
            (
                "Hierarchy.cs",
                """
public class MidException : System.Exception { }
public class LeafError : MidException { }
"""
            )
        );

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.An0001ExceptionNameFormat.Id, DiagnosticSeverity.Warning)
                .WithMessage("LeafError class name should end with Exception")
                .WithSpan("Hierarchy.cs", 2, 14, 2, 23)
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that the analyzer ignores an abstract subclass whose name already ends with
    /// "Exception".
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task AbstractExceptionSubclass_CorrectName_Ignores()
    {
        TestState.Sources.Add(("AbstractEx.cs", "public abstract class FrameworkException : System.Exception { }"));

        ExpectedDiagnostics.Clear();

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that the analyzer does not flag types that are not classes — interfaces whose
    /// name ends with "Exception" do not derive from System.Exception and must not warn.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Interface_Ignores()
    {
        TestState.Sources.Add(("IFoo.cs", "public interface IFooException { }"));

        ExpectedDiagnostics.Clear();

        await RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that the analyzer flags a partial class whose declarations are spread across files
    /// when the name does not end with "Exception".
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task PartialClass_WrongName_ShowsWarning()
    {
        TestState.Sources.Add(("PartA.cs", "public partial class PartialError : System.Exception { }"));
        TestState.Sources.Add(("PartB.cs", "public partial class PartialError { public int Id; }"));

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.An0001ExceptionNameFormat.Id, DiagnosticSeverity.Warning)
                .WithMessage("PartialError class name should end with Exception")
                .WithSpan("PartA.cs", 1, 22, 1, 34)
        );

        await RunAsync(TestContext.Current.CancellationToken);
    }
}
