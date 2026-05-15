using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Tests for <see cref="NumericExtensions"/> — Within/Above/Below on every supported numeric type.
/// Replaces the previous per-type IntExtensions / LongExtensions tests since the methods are
/// collapsed into a single <c>INumber&lt;T&gt;</c> generic surface.
/// </summary>
public class NumericExtensionsTest
{
    /// <summary>Verifies Within clamps an int below the minimum.</summary>
    [Fact]
    public void Within_Int_ClampsBelowMin() => (-5).Within(0, 10).Is(0);

    /// <summary>Verifies Within clamps an int above the maximum.</summary>
    [Fact]
    public void Within_Int_ClampsAboveMax() => 50.Within(0, 10).Is(10);

    /// <summary>Verifies Within passes through an int that is in range.</summary>
    [Fact]
    public void Within_Int_InRange_ReturnsValue() => 5.Within(0, 10).Is(5);

    /// <summary>Verifies Above clamps an int to the minimum.</summary>
    [Fact]
    public void Above_Int_ClampsToMin() => (-3).Above(0).Is(0);

    /// <summary>Verifies Above passes through when the int is already at-or-above min.</summary>
    [Fact]
    public void Above_Int_AlreadyAtOrAbove_ReturnsValue() => 7.Above(0).Is(7);

    /// <summary>Verifies Below clamps an int to the maximum.</summary>
    [Fact]
    public void Below_Int_ClampsToMax() => 100.Below(10).Is(10);

    /// <summary>Verifies Below passes through when the int is at-or-below max.</summary>
    [Fact]
    public void Below_Int_AlreadyAtOrBelow_ReturnsValue() => 3.Below(10).Is(3);

    /// <summary>Verifies Within clamps a long below the minimum.</summary>
    [Fact]
    public void Within_Long_ClampsBelowMin() => (-5L).Within(0L, 10L).Is(0L);

    /// <summary>Verifies Within clamps a long above the maximum.</summary>
    [Fact]
    public void Within_Long_ClampsAboveMax() => 50L.Within(0L, 10L).Is(10L);

    /// <summary>Verifies Within passes through an in-range long.</summary>
    [Fact]
    public void Within_Long_InRange_ReturnsValue() => 5L.Within(0L, 10L).Is(5L);

    /// <summary>Verifies Above on long clamps to the minimum.</summary>
    [Fact]
    public void Above_Long_ClampsToMin() => (-3L).Above(0L).Is(0L);

    /// <summary>Verifies Below on long clamps to the maximum.</summary>
    [Fact]
    public void Below_Long_ClampsToMax() => 100L.Below(10L).Is(10L);

    /// <summary>Verifies Within clamps a double below the minimum.</summary>
    [Fact]
    public void Within_Double_ClampsBelowMin() => (-1.5d).Within(0d, 1d).Is(0d);

    /// <summary>Verifies Within clamps a double above the maximum.</summary>
    [Fact]
    public void Within_Double_ClampsAboveMax() => 2.5d.Within(0d, 1d).Is(1d);

    /// <summary>Verifies Within on float passes the value through when in range.</summary>
    [Fact]
    public void Within_Float_InRange_ReturnsValue() => 0.5f.Within(0f, 1f).Is(0.5f);

    /// <summary>Verifies Within on decimal clamps below the minimum.</summary>
    [Fact]
    public void Within_Decimal_ClampsBelowMin() => (-1.5m).Within(0m, 1m).Is(0m);

    /// <summary>Verifies Within on decimal clamps above the maximum.</summary>
    [Fact]
    public void Within_Decimal_ClampsAboveMax() => 2.5m.Within(0m, 1m).Is(1m);
}
