using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Annium.Analyzers.Tests;

public class ExceptionNameAnalyzerFacts : CSharpAnalyzerTest<ExceptionNameAnalyzer, DefaultVerifier>
{
    [Fact]
    public async Task WhenCorrectName_Ignores()
    {
        TestCode = "public class CustomException : System.Exception { }";

        ExpectedDiagnostics.Clear();

        await RunAsync();
    }

    [Fact]
    public async Task WhenInconsistentName_ShowsWarning()
    {
        TestCode = "public class CustomError : System.Exception { }";

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Pg0001ExceptionNameFormat.Id, DiagnosticSeverity.Warning)
                .WithMessage("CustomError class name should end with Exception")
                .WithSpan(1, 14, 1, 25)
        );

        await RunAsync();
    }
}