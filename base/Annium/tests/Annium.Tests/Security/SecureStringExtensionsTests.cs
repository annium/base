using System.Text;
using Annium.Security;
using Xunit;

namespace Annium.Tests.Security;

public class SecureStringExtensionsTests
{
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