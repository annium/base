using System.IO;
using System.Security.Cryptography;
using Annium.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Tests;

/// <summary>
/// Tests for ECDSA cryptographic key import and validation functionality.
/// Validates the import and properties of both private and public ECDSA keys from PEM format.
/// </summary>
public class EcdsaTests
{
    /// <summary>
    /// Tests the import and validation of ECDSA private key from PEM format.
    /// Verifies key properties including KeyId, PrivateKeyStatus, and KeySize.
    /// </summary>
    [Fact]
    public void PrivateKey()
    {
        // arrange
        var raw = File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"));

        // act
        var key = ECDsa.Create().ImportPem(raw).GetKey();

        // assert
        AssertKeyProperties(key);
    }

    /// <summary>
    /// Tests the import and validation of ECDSA public key from PEM format.
    /// Verifies key properties including KeyId, PrivateKeyStatus, and KeySize.
    /// </summary>
    [Fact]
    public void PublicKey()
    {
        // arrange
        var raw = File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"));

        // act
        var key = ECDsa.Create().ImportPem(raw).GetKey();

        // assert
        AssertKeyProperties(key);
    }

    /// <summary>
    /// Two independently generated ECDSA keys must produce different key identifiers, confirming
    /// that <c>GetKeyId</c> derives the id from the key material rather than a constant or counter.
    /// The created <see cref="ECDsa"/> instances are intentionally not disposed (same rationale as
    /// the JWT test fixtures).
    /// </summary>
    [Fact]
    public void GetKeyId_DistinctKeys_ProduceDifferentIds()
    {
        var id1 = ECDsa.Create().GetKey().KeyId;
        var id2 = ECDsa.Create().GetKey().KeyId;

        id1.IsNotEqual(id2);
    }

    /// <summary>
    /// Asserts the fixture key's identity properties: non-default, the pinned KeyId, the
    /// runtime-reported private-key status (Unknown for ECDsa), and the secp521r1 key size.
    /// </summary>
    /// <param name="key">The imported ECDSA security key.</param>
    private static void AssertKeyProperties(ECDsaSecurityKey key)
    {
        key.IsNotDefault();
        key.KeyId.Is("DPTN:WDOT:XCT7:NHYZ:NSAE:GVQT:OQIX:TCZ6:E3GE:67EY:QA3D:5Q7E");
        // unfortunately, for ECDsa this field is hardcoded to unknown now
        key.PrivateKeyStatus.Is(PrivateKeyStatus.Unknown);
        key.KeySize.Is(521);
    }
}
