using System.IO;
using System.Security.Cryptography;
using Annium.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Tests;

/// <summary>
/// Tests for RSA cryptographic key import and validation functionality.
/// Validates the import and properties of both private and public RSA keys from PEM format.
/// </summary>
public class RsaTests
{
    /// <summary>
    /// Tests the import and validation of RSA private key from PEM format.
    /// Verifies key properties including KeyId, PrivateKeyStatus, and KeySize.
    /// </summary>
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

    /// <summary>
    /// Tests the import and validation of RSA public key from PEM format.
    /// Verifies key properties including KeyId, PrivateKeyStatus, and KeySize.
    /// </summary>
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
