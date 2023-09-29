using Annium.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Linq;

public class EnumerableExtensionsTest
{
    [Fact]
    public void CartesianProduct()
    {
        // arrange
        var a = new[] { 1, 2 };
        var b = new[] { 3, 4 };

        // act
        var result = new[] { a, b }.CartesianProduct();

        // assert
        result.IsEqual(new[]
        {
            new[] { 1, 3 },
            new[] { 1, 4 },
            new[] { 2, 3 },
            new[] { 2, 4 },
        });
    }
}