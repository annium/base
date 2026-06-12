using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Annium.Testing;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
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
        var (privateKey, publicKey) = CreateKeys();

        Works_Base(privateKey, publicKey, SecurityAlgorithms.EcdsaSha512);
    }

    /// <summary>
    /// Regression test: expired token + expirationWindow=null must still return Failed.
    /// </summary>
    [Fact]
    public void Expired_ExpirationWindowNull_Fails()
    {
        var (privateKey, publicKey) = CreateKeys();

        Expired_ExpirationWindowNull_Base(privateKey, publicKey, SecurityAlgorithms.EcdsaSha512);
    }

    /// <summary>
    /// Regression test: expired token + non-null expirationWindow must also return Failed.
    /// </summary>
    [Fact]
    public void Expired_ExpirationWindow_Fails()
    {
        var (privateKey, publicKey) = CreateKeys();

        Expired_ExpirationWindow_Base(privateKey, publicKey, SecurityAlgorithms.EcdsaSha512);
    }

    /// <summary>T6.A: ValidateAudience override = false accepts an audience-mismatched token.</summary>
    [Fact]
    public void Read_WithAudienceValidationDisabled_AcceptsAudienceMismatch()
    {
        var (privateKey, publicKey) = CreateKeys();

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
        var (privateKey, publicKey) = CreateKeys();

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
        var privateKey = LoadPrivateKey();

        Write_WithAudienceOverride_EmitsAudienceClaim_Base(privateKey, SecurityAlgorithms.EcdsaSha512);
    }

    /// <summary>T6.A: Lifetime override drives the emitted exp - iat span.</summary>
    [Fact]
    public void Write_WithLifetimeOverride_EmitsCorrectExpClaim()
    {
        var privateKey = LoadPrivateKey();

        Write_WithLifetimeOverride_EmitsCorrectExpClaim_Base(privateKey, SecurityAlgorithms.EcdsaSha512);
    }

    /// <summary>
    /// When no audience is configured and no write override is supplied, the writer must omit the
    /// <c>aud</c> claim entirely rather than emitting an empty string.
    /// </summary>
    [Fact]
    public void Write_NoAudienceConfigured_OmitsAudClaim()
    {
        var (privateKey, _) = CreateKeys();
        var writer = CreateWriter(privateKey, audience: null);

        var encoded = writer.Write(MinimalPrincipal());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(encoded);
        jwt.Audiences.Any().IsFalse();
    }

    /// <summary>
    /// When a <see cref="JwtWriteOverrides"/> is passed with <c>Audience = null</c>, the writer
    /// falls back to the configured audience rather than suppressing it.
    /// </summary>
    [Fact]
    public void Write_NullAudienceOverride_InheritsConfiguredAudience()
    {
        var (privateKey, _) = CreateKeys();
        var writer = CreateWriter(privateKey, audience: "configured-aud");

        var encoded = writer.Write(MinimalPrincipal(), new JwtWriteOverrides(Audience: null));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(encoded);
        jwt.Audiences.Single().Is("configured-aud");
    }

    /// <summary>
    /// An explicit empty-string audience override omits the aud claim — the writer's
    /// <c>IsNullOrEmpty</c> guard treats <c>""</c> the same as null (no aud claim emitted).
    /// </summary>
    [Fact]
    public void Write_EmptyAudienceOverride_OmitsAudClaim()
    {
        var (privateKey, _) = CreateKeys();
        var writer = CreateWriter(privateKey, audience: "configured-aud");

        var encoded = writer.Write(MinimalPrincipal(), new JwtWriteOverrides(Audience: ""));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(encoded);
        jwt.Audiences.Any().IsFalse();
    }

    /// <summary>
    /// Passing <c>ValidateAudience: true</c> when the reader has no configured audience must throw
    /// <see cref="InvalidOperationException"/> before any token validation occurs.
    /// </summary>
    [Fact]
    public void Read_ForceAudienceValidationWithoutConfiguredAudience_Throws()
    {
        var (privateKey, publicKey) = CreateKeys();
        var writer = CreateWriter(privateKey, audience: null);
        var reader = CreateReader(publicKey, audience: null, expirationWindow: Duration.FromSeconds(10));

        var encoded = writer.Write(MinimalPrincipal());

        Wrap.It(() => reader.Read(encoded, new JwtReadOverrides(ValidateAudience: true)))
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Passing <c>ValidateLifetime: true</c> when the reader has no configured expiration window
    /// must throw <see cref="InvalidOperationException"/> before any token validation occurs.
    /// </summary>
    [Fact]
    public void Read_ForceLifetimeValidationWithoutConfiguredWindow_Throws()
    {
        var (privateKey, publicKey) = CreateKeys();
        var writer = CreateWriter(privateKey, audience: "audience");
        var reader = CreateReader(publicKey, audience: "audience", expirationWindow: null);

        var encoded = writer.Write(MinimalPrincipal());

        Wrap.It(() => reader.Read(encoded, new JwtReadOverrides(ValidateLifetime: true)))
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// A token signed with key-pair A must produce <see cref="TokenReadStatus.InvalidSignature"/>
    /// when read by a reader configured with a different, unrelated public key.
    /// </summary>
    [Fact]
    public void Read_WrongSigningKey_ReturnsInvalidSignature()
    {
        var (privateKey, _) = CreateKeys();
        var writer = CreateWriter(privateKey, audience: "audience");

        var encoded = writer.Write(MinimalPrincipal());

        // Deliberately use a freshly generated key that has no relation to the signing key.
        var wrongKey = ECDsa.Create().GetKey();
        var reader = CreateReader(wrongKey, audience: "audience", expirationWindow: Duration.FromSeconds(10));

        var result = reader.Read(encoded);

        result.Status.Is(TokenReadStatus.InvalidSignature);
        result.Claims.IsNull();
        result.Error.IsNotNull();
    }

    /// <summary>
    /// A token whose issuer does not match the reader's configured issuer must return
    /// <see cref="TokenReadStatus.InvalidClaims"/>.
    /// </summary>
    [Fact]
    public void Read_InvalidIssuer_ReturnsInvalidClaims()
    {
        var (privateKey, publicKey) = CreateKeys();
        var writer = CreateWriter(privateKey, audience: "audience");
        var reader = CreateReader(
            publicKey,
            audience: "audience",
            expirationWindow: Duration.FromSeconds(10),
            issuer: "other-issuer"
        );

        var encoded = writer.Write(MinimalPrincipal());
        var result = reader.Read(encoded);

        result.Status.Is(TokenReadStatus.InvalidClaims);
        result.Claims.IsNull();
        result.Error.IsNotNull();
    }

    /// <summary>
    /// A string without JWT structure (rejected by the CanReadToken guard) must be mapped to
    /// <see cref="TokenReadStatus.Malformed"/> rather than throwing an exception.
    /// </summary>
    [Fact]
    public void Read_MalformedString_ReturnsMalformed()
    {
        var (_, publicKey) = CreateKeys();
        var reader = CreateReader(publicKey, audience: "audience", expirationWindow: Duration.FromSeconds(10));

        // No dot-separated segments → CanReadToken rejects it → Malformed (the documented guard path).
        var result = reader.Read("not-a-jwt");

        result.Status.Is(TokenReadStatus.Malformed);
        result.Claims.IsNull();
        result.Error.IsNotNull();
    }

    /// <summary>
    /// A token whose <c>nbf</c> is in the future must return <see cref="TokenReadStatus.NotYetValid"/>
    /// via the reader's manual ValidFrom check (exercised when ExpirationWindow is null, so the MS
    /// library skips its own lifetime validation).
    /// </summary>
    [Fact]
    public void Read_TokenNotYetValid_ReturnsNotYetValid()
    {
        var (privateKey, publicKey) = CreateKeys();
        var now = DateTime.UtcNow;

        // The writer always stamps nbf = now, so it cannot produce a not-yet-valid token; craft one
        // directly with a future nbf, signed with the private key the reader trusts.
        var header = new JwtHeader(new SigningCredentials(privateKey, SecurityAlgorithms.EcdsaSha512));
        var payload = new JwtPayload(
            "service",
            "audience",
            new[] { new Claim("k", "v") },
            notBefore: now.AddHours(1),
            expires: now.AddHours(2),
            issuedAt: now
        );
        var encoded = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(header, payload));

        // ExpirationWindow = null → MS skips lifetime validation → the manual ValidFrom check runs.
        var reader = CreateReader(publicKey, audience: "audience", expirationWindow: null);

        var result = reader.Read(encoded);

        result.Status.Is(TokenReadStatus.NotYetValid);
        result.Claims.IsNull();
        result.Error.IsNotNull();
    }

    /// <summary>
    /// With a configured ExpirationWindow (MS lifetime validation on), a future-nbf token makes the
    /// MS library throw <c>SecurityTokenNotYetValidException</c>, which the reader maps to
    /// <see cref="TokenReadStatus.NotYetValid"/> via the exception-mapping arm (distinct from the
    /// manual ValidFrom check exercised when ExpirationWindow is null).
    /// </summary>
    [Fact]
    public void Read_TokenNotYetValid_WithExpirationWindow_ReturnsNotYetValid()
    {
        var (privateKey, publicKey) = CreateKeys();
        var now = DateTime.UtcNow;

        var header = new JwtHeader(new SigningCredentials(privateKey, SecurityAlgorithms.EcdsaSha512));
        var payload = new JwtPayload(
            "service",
            "audience",
            new[] { new Claim("k", "v") },
            notBefore: now.AddHours(1),
            expires: now.AddHours(2),
            issuedAt: now
        );
        var encoded = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(header, payload));

        // Non-null ExpirationWindow → MS validates lifetime and throws on the future nbf.
        var reader = CreateReader(publicKey, audience: "audience", expirationWindow: Duration.FromSeconds(10));

        var result = reader.Read(encoded);

        result.Status.Is(TokenReadStatus.NotYetValid);
        result.Claims.IsNull();
        result.Error.IsNotNull();
    }

    /// <summary>
    /// With no configured ExpirationWindow, an explicit <c>ValidateLifetime: false</c> override must
    /// accept an already-expired token: both the MS lifetime check and the manual ValidFrom/ValidTo
    /// check are suppressed (the refresh-token-on-unconfigured-window path).
    /// </summary>
    [Fact]
    public void Read_LifetimeValidationDisabledWithoutWindow_AcceptsExpiredToken()
    {
        var (privateKey, publicKey) = CreateKeys();
        var now = DateTime.UtcNow;

        // The writer always stamps exp = now + lifetime, so craft an already-expired token directly.
        var header = new JwtHeader(new SigningCredentials(privateKey, SecurityAlgorithms.EcdsaSha512));
        var payload = new JwtPayload(
            "service",
            "audience",
            new[] { new Claim("k", "v") },
            notBefore: now.AddHours(-2),
            expires: now.AddHours(-1),
            issuedAt: now.AddHours(-2)
        );
        var encoded = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(header, payload));

        var reader = CreateReader(publicKey, audience: "audience", expirationWindow: null);

        var result = reader.Read(encoded, new JwtReadOverrides(ValidateLifetime: false));

        result.Status.Is(TokenReadStatus.Ok);
    }

    /// <summary>
    /// Builds an ECDSA-signing <see cref="JwtWriter"/> over a fresh real-time provider, with the
    /// algorithm fixed to ECDSA-SHA512 and the issuer defaulting to <c>"service"</c>.
    /// </summary>
    /// <param name="key">Signing key.</param>
    /// <param name="audience">Configured audience (null = none emitted).</param>
    /// <param name="issuer">Token issuer.</param>
    /// <param name="lifetime">Token lifetime (defaults to 45 seconds).</param>
    /// <returns>The configured writer.</returns>
    private static JwtWriter CreateWriter(
        SecurityKey key,
        string? audience,
        string issuer = "service",
        Duration? lifetime = null
    ) =>
        new(
            new JwtTokensOptions
            {
                SigningKey = key,
                Algorithm = SecurityAlgorithms.EcdsaSha512,
                Issuer = issuer,
                Audience = audience,
                Lifetime = lifetime ?? Duration.FromSeconds(45),
            },
            ResolveTime()
        );

    /// <summary>
    /// Builds an ECDSA-verifying <see cref="JwtReader"/> over a fresh real-time provider, with the
    /// algorithm fixed to ECDSA-SHA512 and the issuer defaulting to <c>"service"</c>.
    /// </summary>
    /// <param name="key">Verification key.</param>
    /// <param name="audience">Configured audience (null = audience check disabled).</param>
    /// <param name="expirationWindow">Clock-skew tolerance (null = manual ValidFrom/ValidTo check).</param>
    /// <param name="issuer">Expected issuer.</param>
    /// <returns>The configured reader.</returns>
    private static JwtReader CreateReader(
        SecurityKey key,
        string? audience,
        Duration? expirationWindow,
        string issuer = "service"
    ) =>
        new(
            new JwtTokensOptions
            {
                SigningKey = key,
                Algorithm = SecurityAlgorithms.EcdsaSha512,
                Issuer = issuer,
                Audience = audience,
                ExpirationWindow = expirationWindow,
                Lifetime = Duration.FromSeconds(45),
            },
            ResolveTime()
        );

    /// <summary>
    /// Loads the ECDSA private/public key pair from the test fixture PEM files. The created
    /// <see cref="ECDsa"/> instances are intentionally not disposed — Microsoft.IdentityModel caches
    /// signature providers by KeyId for the process lifetime, so disposing them breaks later tests.
    /// </summary>
    /// <returns>The private and public ECDSA security keys.</returns>
    private static (ECDsaSecurityKey privateKey, ECDsaSecurityKey publicKey) CreateKeys()
    {
        var privateKey = LoadPrivateKey();
        var publicKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"))).GetKey();

        return (privateKey, publicKey);
    }

    /// <summary>
    /// Loads the ECDSA private key from the test fixture PEM file. The created <see cref="ECDsa"/> is
    /// intentionally not disposed (see <see cref="CreateKeys"/> for the rationale).
    /// </summary>
    /// <returns>The private ECDSA security key.</returns>
    private static ECDsaSecurityKey LoadPrivateKey() =>
        ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();
}
