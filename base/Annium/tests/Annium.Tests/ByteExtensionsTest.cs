using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class ByteExtensionsTest
{
    [Fact]
    public void ToHexString_Works()
    {
        // arrange
        var byteArray = new byte[] { 7, 220, 34 };

        // act
        var str = byteArray.ToHexString();

        // assert
        str.IsEqual("07DC22");
    }
}