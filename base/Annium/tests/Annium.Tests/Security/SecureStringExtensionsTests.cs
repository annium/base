using System.Text;
using Annium.Security;
using Xunit;

namespace Annium.Tests.Security;

/// <summary>
/// Contains unit tests for the SecureStringExtensions class.
/// </summary>
public class SecureStringExtensionsTests
{
    /// <summary>
    /// Verifies that encoding a string to SecureString and decoding it back works correctly.
    /// </summary>
    [Fact]
    public void Encode_Decode_Works()
    {
        // arrange
        var source = "sample*$&тест123";

        // encode
        var encoded = source.AsSecureString();

        // decode
        var decoded = Encoding.UTF8.GetString(encoded.AsBytes());

        Assert.Equal(source, decoded);
    }
}
