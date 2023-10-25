using System.IO;
using System.Security.Cryptography;
using Annium.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Tests;

public class RsaTests
{
    [Fact]
    public void PrivateKey()
    {
        // arrange
        var raw = File.ReadAllText(Path.Combine("keys", "rsa_private.pem"));

        // act
        var key = RSA.Create().ImportPem(raw).GetKey();

        // assert
        key.IsNotDefault();
        key.KeyId.Is("3PRM:GCC2:G2M2:OTXW:AMG5:OD6L:B7AM:UPKV:7WKO:GEMW:D5S7:DZBZ");
        key.PrivateKeyStatus.Is(PrivateKeyStatus.Exists);
        key.KeySize.Is(2048);
    }

    [Fact]
    public void PublicKey()
    {
        // arrange
        var raw = File.ReadAllText(Path.Combine("keys", "rsa_public.pem"));

        // act
        var key = RSA.Create().ImportPem(raw).GetKey();

        // assert
        key.IsNotDefault();
        key.KeyId.Is("3PRM:GCC2:G2M2:OTXW:AMG5:OD6L:B7AM:UPKV:7WKO:GEMW:D5S7:DZBZ");
        key.PrivateKeyStatus.Is(PrivateKeyStatus.Unknown);
        key.KeySize.Is(2048);
    }
}