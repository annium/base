using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for byte array extension methods.
/// </summary>
public class ByteExtensionsTest
{
    /// <summary>
    /// Verifies that the ToHexString extension method correctly converts a byte array to a hexadecimal string.
    /// </summary>
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
