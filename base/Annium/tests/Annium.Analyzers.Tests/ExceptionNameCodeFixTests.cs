using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Annium.Analyzers.Tests;

public class ExceptionNameCodeFixTests : CSharpCodeFixTest<ExceptionNameAnalyzer, ExceptionNameCodeFix, DefaultVerifier>
{
    [Fact]
    public async Task WhenInconsistentName_AddsExceptionPostfix()
    {
        TestState.Sources.Add(("CustomError.cs", "public class CustomError : System.Exception { }"));

        ExpectedDiagnostics.Add(
            new DiagnosticResult(Descriptors.Pg0001ExceptionNameFormat.Id, DiagnosticSeverity.Warning)
                .WithArguments("CustomError")
                .WithMessageFormat(Descriptors.Pg0001ExceptionNameFormat.MessageFormat)
                .WithSpan("CustomError.cs", 1, 14, 1, 25)
        );

        FixedState.Sources.Add(("CustomErrorException.cs", "public class CustomErrorException : System.Exception { }"));

        await RunAsync();
    }
}
