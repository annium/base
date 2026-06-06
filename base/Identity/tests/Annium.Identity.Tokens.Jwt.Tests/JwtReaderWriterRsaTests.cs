using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Jwt.Tests;

/// <summary>
/// Tests for JWT reader and writer functionality using RSA cryptographic algorithm.
/// Validates JWT token creation, signing, and verification with RSA keys.
/// </summary>
public class JwtReaderWriterRsaTests : JwtReaderWriterTestsBase
{
    /// <summary>
    /// Tests JWT token creation and reading with RSA cryptographic algorithm.
    /// Verifies that tokens signed with RSA private key can be validated with corresponding public key.
    /// </summary>
    [Fact]
    public void Works()
    {
        var (privateKey, publicKey) = CreateKeys();

        Works_Base(privateKey, publicKey, SecurityAlgorithms.RsaSha256);
    }

    /// <summary>
    /// Regression test: expired token + expirationWindow=null must still return Failed.
    /// </summary>
    [Fact]
    public void Expired_ExpirationWindowNull_Fails()
    {
        var (privateKey, publicKey) = CreateKeys();

        Expired_ExpirationWindowNull_Base(privateKey, publicKey, SecurityAlgorithms.RsaSha256);
    }

    /// <summary>
    /// Regression test: expired token + non-null expirationWindow must also return Failed.
    /// </summary>
    [Fact]
    public void Expired_ExpirationWindow_Fails()
    {
        var (privateKey, publicKey) = CreateKeys();

        Expired_ExpirationWindow_Base(privateKey, publicKey, SecurityAlgorithms.RsaSha256);
    }

    /// <summary>T6.A: ValidateAudience override = false accepts an audience-mismatched token (RSA).</summary>
    [Fact]
    public void Read_WithAudienceValidationDisabled_AcceptsAudienceMismatch()
    {
        var (privateKey, publicKey) = CreateKeys();

        Read_WithAudienceValidationDisabled_AcceptsAudienceMismatch_Base(
            privateKey,
            publicKey,
            SecurityAlgorithms.RsaSha256
        );
    }

    /// <summary>T6.A: ValidateLifetime override = false accepts an expired token (RSA).</summary>
    [Fact]
    public void Read_WithLifetimeValidationDisabled_AcceptsExpiredToken()
    {
        var (privateKey, publicKey) = CreateKeys();

        Read_WithLifetimeValidationDisabled_AcceptsExpiredToken_Base(
            privateKey,
            publicKey,
            SecurityAlgorithms.RsaSha256
        );
    }

    /// <summary>T6.A: Audience override drives the emitted aud claim (RSA).</summary>
    [Fact]
    public void Write_WithAudienceOverride_EmitsAudienceClaim()
    {
        var (privateKey, _) = CreateKeys();

        Write_WithAudienceOverride_EmitsAudienceClaim_Base(privateKey, SecurityAlgorithms.RsaSha256);
    }

    /// <summary>T6.A: Lifetime override drives the emitted exp - iat span (RSA).</summary>
    [Fact]
    public void Write_WithLifetimeOverride_EmitsCorrectExpClaim()
    {
        var (privateKey, _) = CreateKeys();

        Write_WithLifetimeOverride_EmitsCorrectExpClaim_Base(privateKey, SecurityAlgorithms.RsaSha256);
    }

    /// <summary>
    /// Loads the RSA private/public key pair from the test fixture PEM files. The created
    /// <see cref="RSA"/> instances are intentionally not disposed — Microsoft.IdentityModel caches
    /// signature providers by KeyId for the process lifetime, so disposing them breaks later tests.
    /// </summary>
    /// <returns>The private and public RSA security keys.</returns>
    private static (RsaSecurityKey privateKey, RsaSecurityKey publicKey) CreateKeys()
    {
        var privateKey = RSA.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "rsa_private.pem"))).GetKey();
        var publicKey = RSA.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "rsa_public.pem"))).GetKey();

        return (privateKey, publicKey);
    }
}
