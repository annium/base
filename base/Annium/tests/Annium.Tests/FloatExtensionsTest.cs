using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for float extension methods.
/// </summary>
public class FloatExtensionsTest
{
    /// <summary>
    /// Verifies that the DiffFrom extension method works correctly.
    /// </summary>
    [Fact]
    public void DiffFrom()
    {
        // arrange
        var a = 9f;
        var b = -9f;
        var c = 0f;
        var d = 10f;
        var e = -10f;

        // assert
        a.DiffFrom(a).IsDefault();
        a.DiffFrom(b).Is(2f);
        a.DiffFrom(c).Is(float.PositiveInfinity);
        a.DiffFrom(d).Is(0.1f);
        a.DiffFrom(e).Is(1.9f);
        0f.DiffFrom(0f).Is(0f);
    }
}
