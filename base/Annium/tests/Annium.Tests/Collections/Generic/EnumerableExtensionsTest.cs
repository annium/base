using System.Linq;
using Annium.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.Generic;

public class EnumerableExtensionsTest
{
    [Fact]
    public void Chunks_Works()
    {
        // arrange
        var data = new[] { 1, 2, 3, 4, 5 };

        // act
        var chunks = data.Chunks(2).ToArray();

        // assert
        chunks.IsEqual(new[] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5 } });
    }
}