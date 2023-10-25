using System.IO;
using System.Security.Cryptography;
using Annium.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Tests;

public class EcdsaTests
{
    [Fact]
    public void PrivateKey()
    {
        // arrange
        var raw = File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"));

        // act
        var key = ECDsa.Create().ImportPem(raw).GetKey();

        // assert
        key.IsNotDefault();
        key.KeyId.Is("DPTN:WDOT:XCT7:NHYZ:NSAE:GVQT:OQIX:TCZ6:E3GE:67EY:QA3D:5Q7E");
        // unfortunately, for ECDsa this field is hardcoded to unknown now
        key.PrivateKeyStatus.Is(PrivateKeyStatus.Unknown);
        key.KeySize.Is(521);
    }

    [Fact]
    public void PublicKey()
    {
        // arrange
        var raw = File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"));

        // act
        var key = ECDsa.Create().ImportPem(raw).GetKey();

        // assert
        key.IsNotDefault();
        key.KeyId.Is("DPTN:WDOT:XCT7:NHYZ:NSAE:GVQT:OQIX:TCZ6:E3GE:67EY:QA3D:5Q7E");
        key.PrivateKeyStatus.Is(PrivateKeyStatus.Unknown);
        key.KeySize.Is(521);
    }
}