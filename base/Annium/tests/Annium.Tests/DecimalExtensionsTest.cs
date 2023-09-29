using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class DecimalExtensionsTest
{
    [Fact]
    public void DiffFrom()
    {
        // arrange
        var a = 0.99m;
        var b = -0.99m;
        var c = 0m;
        var d = 1m;
        var e = -1m;

        // assert
        a.DiffFrom(a).IsDefault();
        a.DiffFrom(b).Is(2m);
        a.DiffFrom(c).Is(decimal.MaxValue);
        a.DiffFrom(d).Is(0.01m);
        a.DiffFrom(e).Is(1.99m);
        0m.DiffFrom(0m).Is(0m);
    }

    [Fact]
    public void ToPretty()
    {
        // assert
        123.9214999m.ToPretty(0.01m).Is(124m);
        123.9214999m.ToPretty(0.1m).Is(120m);
        123.9214999m.ToPretty(0.5m).Is(100m);
    }

    [Fact]
    public void FloorTo()
    {
        // assert
        123.21m.FloorTo(7.6m).Is(121.6m);
        123.21m.FloorTo(0.2m).Is(123.2m);
    }

    [Fact]
    public void CeilTo()
    {
        // assert
        123.21m.CeilTo(7.6m).Is(129.2m);
        123.21m.CeilTo(0.2m).Is(123.4m);
    }

    [Fact]
    public void Align()
    {
        // assert
        123m.Align().Decimals().Is(0);
        123.21m.Align().Decimals().Is(2);
        123.2100m.Align().Decimals().Is(2);
    }
}