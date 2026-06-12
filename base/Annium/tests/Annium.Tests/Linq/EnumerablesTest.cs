using System.Linq;
using Annium.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Linq;

/// <summary>
/// Tests for <see cref="Enumerables"/> factory methods.
/// </summary>
public class EnumerablesTest
{
    /// <summary>Range&lt;int&gt; with step 1 produces consecutive integers.</summary>
    [Fact]
    public void Range_Int_StepOne_ProducesConsecutive() =>
        Enumerables.Range(5, 4, 1).ToArray().IsEqual(new[] { 5, 6, 7, 8 });

    /// <summary>Range&lt;int&gt; with a multi step skips by that amount.</summary>
    [Fact]
    public void Range_Int_MultiStep_SkipsByStep() => Enumerables.Range(0, 4, 3).ToArray().IsEqual(new[] { 0, 3, 6, 9 });

    /// <summary>Range&lt;int&gt; with a negative step counts down.</summary>
    [Fact]
    public void Range_Int_NegativeStep_CountsDown() =>
        Enumerables.Range(10, 3, -2).ToArray().IsEqual(new[] { 10, 8, 6 });

    /// <summary>Range&lt;int&gt; with zero count produces an empty sequence.</summary>
    [Fact]
    public void Range_Int_ZeroCount_Empty() => Enumerables.Range(0, 0, 1).ToArray().Length.Is(0);

    /// <summary>Range&lt;decimal&gt; with a fractional step produces the expected sequence.</summary>
    [Fact]
    public void Range_Decimal_FractionalStep_ProducesSequence() =>
        Enumerables.Range(0m, 5, 0.25m).ToArray().IsEqual(new[] { 0m, 0.25m, 0.50m, 0.75m, 1.00m });

    /// <summary>Range&lt;decimal&gt; with a negative step counts down.</summary>
    [Fact]
    public void Range_Decimal_NegativeStep_CountsDown() =>
        Enumerables.Range(1m, 3, -0.5m).ToArray().IsEqual(new[] { 1m, 0.5m, 0m });
}
