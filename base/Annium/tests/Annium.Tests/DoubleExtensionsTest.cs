using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class DoubleExtensionsTest
{
    [Fact]
    public void DiffFrom()
    {
        // arrange
        var a = 9d;
        var b = -9d;
        var c = 0d;
        var d = 10d;
        var e = -10d;

        // assert
        a.DiffFrom(a).IsDefault();
        a.DiffFrom(b).Is(2d);
        a.DiffFrom(c).Is(float.PositiveInfinity);
        a.DiffFrom(d).Is(0.1d);
        a.DiffFrom(e).Is(1.9d);
        0d.DiffFrom(0d).Is(0d);
    }
}