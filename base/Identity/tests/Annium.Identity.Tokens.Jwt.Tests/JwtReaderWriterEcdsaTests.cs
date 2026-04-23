using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Jwt.Tests;

/// <summary>
/// Tests for JWT reader and writer functionality using ECDSA cryptographic algorithm.
/// Validates JWT token creation, signing, and verification with ECDSA keys.
/// </summary>
public class JwtReaderWriterEcdsaTests : JwtReaderWriterTestsBase
{
    /// <summary>
    /// Tests JWT token creation and reading with ECDSA cryptographic algorithm.
    /// Verifies that tokens signed with ECDSA private key can be validated with corresponding public key.
    /// </summary>
    [Fact]
    public void Works()
    {
        var privateKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();
        var publicKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"))).GetKey();

        Works_Base(privateKey, publicKey, SecurityAlgorithms.EcdsaSha512);
    }

    /// <summary>
    /// Regression test: expired token + expirationWindow=null must still return Failed.
    /// </summary>
    [Fact]
    public void Expired_ExpirationWindowNull_Fails()
    {
        var privateKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();
        var publicKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"))).GetKey();

        Expired_ExpirationWindowNull_Base(privateKey, publicKey, SecurityAlgorithms.EcdsaSha512);
    }

    /// <summary>
    /// Regression test: expired token + non-null expirationWindow must also return Failed.
    /// </summary>
    [Fact]
    public void Expired_ExpirationWindow_Fails()
    {
        var privateKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();
        var publicKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"))).GetKey();

        Expired_ExpirationWindow_Base(privateKey, publicKey, SecurityAlgorithms.EcdsaSha512);
    }
}
