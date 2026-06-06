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
        AssertKeyProperties(key, PrivateKeyStatus.Exists);
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
        AssertKeyProperties(key, PrivateKeyStatus.Unknown);
    }

    /// <summary>
    /// Two independently generated RSA keys must produce different key identifiers, confirming
    /// that <c>GetKeyId</c> derives the id from the key material rather than a constant or counter.
    /// The created <see cref="RSA"/> instances are intentionally not disposed (same rationale as
    /// the JWT test fixtures).
    /// </summary>
    [Fact]
    public void GetKeyId_DistinctKeys_ProduceDifferentIds()
    {
        var id1 = RSA.Create().GetKey().KeyId;
        var id2 = RSA.Create().GetKey().KeyId;

        id1.IsNotEqual(id2);
    }

    /// <summary>
    /// Asserts the fixture key's identity properties: non-default, the pinned KeyId, the given
    /// private-key status, and the 2048-bit key size.
    /// </summary>
    /// <param name="key">The imported RSA security key.</param>
    /// <param name="privateKeyStatus">Expected status (Exists for the private key, Unknown for the public).</param>
    private static void AssertKeyProperties(RsaSecurityKey key, PrivateKeyStatus privateKeyStatus)
    {
        key.IsNotDefault();
        key.KeyId.Is("3PRM:GCC2:G2M2:OTXW:AMG5:OD6L:B7AM:UPKV:7WKO:GEMW:D5S7:DZBZ");
        key.PrivateKeyStatus.Is(privateKeyStatus);
        key.KeySize.Is(2048);
    }
}
