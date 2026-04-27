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

    /// <summary>T6.A: ValidateAudience override = false accepts an audience-mismatched token.</summary>
    [Fact]
    public void Read_WithAudienceValidationDisabled_AcceptsAudienceMismatch()
    {
        var privateKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();
        var publicKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"))).GetKey();

        Read_WithAudienceValidationDisabled_AcceptsAudienceMismatch_Base(
            privateKey,
            publicKey,
            SecurityAlgorithms.EcdsaSha512
        );
    }

    /// <summary>T6.A: ValidateLifetime override = false accepts an expired token.</summary>
    [Fact]
    public void Read_WithLifetimeValidationDisabled_AcceptsExpiredToken()
    {
        var privateKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();
        var publicKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"))).GetKey();

        Read_WithLifetimeValidationDisabled_AcceptsExpiredToken_Base(
            privateKey,
            publicKey,
            SecurityAlgorithms.EcdsaSha512
        );
    }

    /// <summary>T6.A: Audience override drives the emitted aud claim.</summary>
    [Fact]
    public void Write_WithAudienceOverride_EmitsAudienceClaim()
    {
        var privateKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();

        Write_WithAudienceOverride_EmitsAudienceClaim_Base(privateKey, SecurityAlgorithms.EcdsaSha512);
    }

    /// <summary>T6.A: Lifetime override drives the emitted exp - iat span.</summary>
    [Fact]
    public void Write_WithLifetimeOverride_EmitsCorrectExpClaim()
    {
        var privateKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();

        Write_WithLifetimeOverride_EmitsCorrectExpClaim_Base(privateKey, SecurityAlgorithms.EcdsaSha512);
    }
}
