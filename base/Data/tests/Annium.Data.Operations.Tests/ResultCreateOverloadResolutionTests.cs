using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Tests;

/// <summary>
/// Verifies that the renamed <see cref="Result.Create"/> overloads resolve to the expected
/// concrete result types. This guards against regressions if the factory surface is reshuffled.
/// </summary>
public class ResultCreateOverloadResolutionTests
{
    /// <summary>
    /// <c>Result.Create()</c> with no args returns an <see cref="IResult"/> instance.
    /// </summary>
    [Fact]
    public void Create_NoArgs_ReturnsIResult()
    {
        var r = Result.Create();
        r.IsNotDefault();
        // r is IResult — compile-time check via assignment
        IResult typed = r;
        typed.IsOk.IsTrue();
    }

    /// <summary>
    /// <c>Result.Create&lt;TD&gt;(data)</c> with a reference-type argument returns
    /// <see cref="IResult{TD}"/>.
    /// </summary>
    [Fact]
    public void Create_ReferenceData_ReturnsIResultOfTd()
    {
        var r = Result.Create("hello");
        IResult<string> typed = r;
        typed.Data.Is("hello");
    }

    /// <summary>
    /// <c>Result.Create&lt;TD&gt;(data)</c> with a value-type argument returns
    /// <see cref="IResult{TD}"/>.
    /// </summary>
    [Fact]
    public void Create_ValueData_ReturnsIResultOfTd()
    {
        var r = Result.Create(42);
        IResult<int> typed = r;
        typed.Data.Is(42);
    }
}
