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
        key.IsNotDefault();
        key.KeyId.Is("DPTN:WDOT:XCT7:NHYZ:NSAE:GVQT:OQIX:TCZ6:E3GE:67EY:QA3D:5Q7E");
        // unfortunately, for ECDsa this field is hardcoded to unknown now
        key.PrivateKeyStatus.Is(PrivateKeyStatus.Unknown);
        key.KeySize.Is(521);
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
        key.IsNotDefault();
        key.KeyId.Is("DPTN:WDOT:XCT7:NHYZ:NSAE:GVQT:OQIX:TCZ6:E3GE:67EY:QA3D:5Q7E");
        key.PrivateKeyStatus.Is(PrivateKeyStatus.Unknown);
        key.KeySize.Is(521);
    }
}
