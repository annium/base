using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for array deconstruction extension methods.
/// </summary>
public class ArrayExtensionsTest : TestBase
{
    public ArrayExtensionsTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies deconstruction with 1 element and the rest.
    /// </summary>
    [Fact]
    public void Deconstruct_1_Works()
    {
        // arrange
        var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // act & assert

        var (x1, rest) = arr;
        x1.Is(1);
        rest.IsEqual(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 });
    }

    /// <summary>
    /// Verifies deconstruction with 2 elements and the rest.
    /// </summary>
    [Fact]
    public void Deconstruct_2_Works()
    {
        // arrange
        var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // act & assert

        var (x1, x2, rest) = arr;
        x1.Is(1);
        x2.Is(2);
        rest.IsEqual(new[] { 3, 4, 5, 6, 7, 8, 9, 10 });
    }

    /// <summary>
    /// Verifies deconstruction with 3 elements and the rest.
    /// </summary>
    [Fact]
    public void Deconstruct_3_Works()
    {
        // arrange
        var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // act & assert

        var (x1, x2, x3, rest) = arr;
        x1.Is(1);
        x2.Is(2);
        x3.Is(3);
        rest.IsEqual(new[] { 4, 5, 6, 7, 8, 9, 10 });
    }

    /// <summary>
    /// Verifies deconstruction with 4 elements and the rest.
    /// </summary>
    [Fact]
    public void Deconstruct_4_Works()
    {
        // arrange
        var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // act & assert

        var (x1, x2, x3, x4, rest) = arr;
        x1.Is(1);
        x2.Is(2);
        x3.Is(3);
        x4.Is(4);
        rest.IsEqual(new[] { 5, 6, 7, 8, 9, 10 });
    }

    /// <summary>
    /// Verifies deconstruction with 5 elements and the rest.
    /// </summary>
    [Fact]
    public void Deconstruct_5_Works()
    {
        // arrange
        var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // act & assert

        var (x1, x2, x3, x4, x5, rest) = arr;
        x1.Is(1);
        x2.Is(2);
        x3.Is(3);
        x4.Is(4);
        x5.Is(5);
        rest.IsEqual(new[] { 6, 7, 8, 9, 10 });
    }

    /// <summary>
    /// Verifies deconstruction with 6 elements and the rest.
    /// </summary>
    [Fact]
    public void Deconstruct_6_Works()
    {
        // arrange
        var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // act & assert

        var (x1, x2, x3, x4, x5, x6, rest) = arr;
        x1.Is(1);
        x2.Is(2);
        x3.Is(3);
        x4.Is(4);
        x5.Is(5);
        x6.Is(6);
        rest.IsEqual(new[] { 7, 8, 9, 10 });
    }

    /// <summary>
    /// Verifies deconstruction with 7 elements and the rest.
    /// </summary>
    [Fact]
    public void Deconstruct_7_Works()
    {
        // arrange
        var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // act & assert

        var (x1, x2, x3, x4, x5, x6, x7, rest) = arr;
        x1.Is(1);
        x2.Is(2);
        x3.Is(3);
        x4.Is(4);
        x5.Is(5);
        x6.Is(6);
        x7.Is(7);
        rest.IsEqual(new[] { 8, 9, 10 });
    }

    /// <summary>
    /// Verifies deconstruction with 8 elements and the rest.
    /// </summary>
    [Fact]
    public void Deconstruct_8_Works()
    {
        // arrange
        var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // act & assert

        var (x1, x2, x3, x4, x5, x6, x7, x8, rest) = arr;
        x1.Is(1);
        x2.Is(2);
        x3.Is(3);
        x4.Is(4);
        x5.Is(5);
        x6.Is(6);
        x7.Is(7);
        x8.Is(8);
        rest.IsEqual(new[] { 9, 10 });
    }
}
