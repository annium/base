using System.IO;
using Annium.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Tests;

public class KeyReaderTests
{
    [Fact]
    public void ReadRsaKey_PrivateKey_Works()
    {
        // arrange
        var raw = File.ReadAllText(Path.Combine("keys", "private.key"));

        // act
        var key = KeyReader.ReadRsaKey(raw);

        // assert
        key.IsNotDefault();
        key.KeyId.Is("Z74D:P6CY:GVHF:VUSX:DSXJ:L27Z:SHIG:4VIB:JC7D:EHMH:PJ3S:AYOV");
        key.PrivateKeyStatus.Is(PrivateKeyStatus.Exists);
        key.KeySize.Is(2048);
    }

    [Fact]
    public void ReadRsaKey_PublicKey_Works()
    {
        // arrange
        var raw = File.ReadAllText(Path.Combine("keys", "public.key"));

        // act
        var key = KeyReader.ReadRsaKey(raw);

        // assert
        key.IsNotDefault();
        key.KeyId.Is("Z74D:P6CY:GVHF:VUSX:DSXJ:L27Z:SHIG:4VIB:JC7D:EHMH:PJ3S:AYOV");
        key.PrivateKeyStatus.Is(PrivateKeyStatus.Unknown);
        key.KeySize.Is(2048);
    }
}