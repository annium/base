using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Testing;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace Annium.Identity.Tokens.Jwt.Tests;

/// <summary>
/// Base class providing common test functionality for JWT reader and writer operations.
/// Uses the T8-introduced instance API: <see cref="ITokenReader{TClaims}"/> /
/// <see cref="ITokenWriter{TClaims}"/> resolved from the DI container via
/// <see cref="ServiceContainerExtensions.AddJwtTokens{TContainer}"/>.
/// </summary>
public class JwtReaderWriterTestsBase
{
    /// <summary>
    /// Builds a configured DI container and resolves the reader + writer pair for tests to exercise.
    /// </summary>
    /// <param name="signingKey">Signing key (private for writer; public for reader).</param>
    /// <param name="algorithm">Signing algorithm.</param>
    /// <param name="issuer">Token issuer.</param>
    /// <param name="audience">Optional token audience.</param>
    /// <param name="expirationWindow">Optional clock-skew tolerance.</param>
    /// <param name="lifetime">Token lifetime applied by the writer.</param>
    /// <returns>Tuple of resolved reader, writer, and the container's time provider.</returns>
    private static (
        ITokenReader<ClaimsPrincipal> reader,
        ITokenWriter<ClaimsPrincipal> writer,
        ITimeProvider time
    ) Resolve(
        SecurityKey signingKey,
        string algorithm,
        string issuer,
        string? audience,
        Duration? expirationWindow,
        Duration lifetime
    )
    {
        var container = new ServiceContainer();
        container.AddRuntime(typeof(JwtReaderWriterTestsBase).Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddJwtTokens(o =>
        {
            o.SigningKey = signingKey;
            o.Algorithm = algorithm;
            o.Issuer = issuer;
            o.Audience = audience;
            o.ExpirationWindow = expirationWindow;
            o.Lifetime = lifetime;
        });

        var sp = container.BuildServiceProvider();
        return (
            sp.Resolve<ITokenReader<ClaimsPrincipal>>(),
            sp.Resolve<ITokenWriter<ClaimsPrincipal>>(),
            sp.Resolve<ITimeProvider>()
        );
    }

    /// <summary>
    /// Round-trip a token: write a principal, read it back, verify the claims survive.
    /// </summary>
    /// <param name="privateKey">The private key used for token signing.</param>
    /// <param name="publicKey">The public key used for token validation.</param>
    /// <param name="signatureAlgorithm">The cryptographic algorithm for signing.</param>
    protected void Works_Base(SecurityKey privateKey, SecurityKey publicKey, string signatureAlgorithm)
    {
        // arrange
        var tokenId = Guid.NewGuid().ToString();
        var issuer = "service";
        var audience = "audience";
        var lifetime = Duration.FromSeconds(45);
        var key = "sample";
        var data = "g87asgdf";

        var (_, writer, _) = Resolve(
            privateKey,
            signatureAlgorithm,
            issuer,
            audience,
            Duration.FromSeconds(10),
            lifetime
        );
        var (reader, _, _) = Resolve(
            publicKey,
            signatureAlgorithm,
            issuer,
            audience,
            Duration.FromSeconds(10),
            lifetime
        );

        var inputClaims = new ClaimsIdentity(new[] { new Claim("jti", tokenId), new Claim(key, data) }, "JWT");
        var inputPrincipal = new ClaimsPrincipal(inputClaims);

        // act - write
        var encoded = writer.Write(inputPrincipal);

        // assert - encoded is non-empty
        encoded.IsNotDefault();

        // act - read
        var result = reader.Read(encoded);

        // assert - read
        result.Status.Is(TokenReadStatus.Ok);
        result.Error.IsNull();
        result.Claims.IsNotDefault();
        var claims = result.Claims!.Claims.ToArray();
        claims.FirstOrDefault(c => c.Type == "jti").IsNotDefault().Value.Is(tokenId);
        claims.FirstOrDefault(c => c.Type == key).IsNotDefault().Value.Is(data);
        claims.FirstOrDefault(c => c.Type == "iss").IsNotDefault().Value.Is(issuer);
        claims.FirstOrDefault(c => c.Type == "aud").IsNotDefault().Value.Is(audience);
    }

    /// <summary>
    /// Regression for plan §2.9: reading an already-expired token must fail regardless of
    /// whether the caller passed an <c>ExpirationWindow</c>. With <c>ExpirationWindow == null</c>,
    /// MS skips the lifetime check; the manual ValidFrom/ValidTo enforcement in
    /// <see cref="JwtReader"/> handles it.
    /// </summary>
    /// <param name="privateKey">The private key used for token signing.</param>
    /// <param name="publicKey">The public key used for token validation.</param>
    /// <param name="signatureAlgorithm">The cryptographic algorithm for signing.</param>
    protected void Expired_ExpirationWindowNull_Base(
        SecurityKey privateKey,
        SecurityKey publicKey,
        string signatureAlgorithm
    )
    {
        // arrange — write a token with a 1 ms lifetime so it expires almost immediately
        var issuer = "service";
        var audience = "audience";
        var lifetime = Duration.FromMilliseconds(1); // immediately expires

        var (_, writer, _) = Resolve(
            privateKey,
            signatureAlgorithm,
            issuer,
            audience,
            Duration.FromSeconds(10),
            lifetime
        );
        var encoded = writer.Write(MinimalPrincipal());

        // wait so the token's ValidTo (now + 1ms) is in the past
        Thread.Sleep(50);

        var (reader, _, _) = Resolve(publicKey, signatureAlgorithm, issuer, audience, expirationWindow: null, lifetime);

        // act
        var result = reader.Read(encoded);

        // assert — must reject as Expired even though ValidateLifetime is off
        result.Status.Is(TokenReadStatus.Expired);
    }

    /// <summary>
    /// Mirror with non-null <c>ExpirationWindow</c> — MS library throws and the reader maps
    /// the exception to <see cref="TokenReadStatus.Expired"/>.
    /// </summary>
    /// <param name="privateKey">The private key used for token signing.</param>
    /// <param name="publicKey">The public key used for token validation.</param>
    /// <param name="signatureAlgorithm">The cryptographic algorithm for signing.</param>
    protected void Expired_ExpirationWindow_Base(
        SecurityKey privateKey,
        SecurityKey publicKey,
        string signatureAlgorithm
    )
    {
        var issuer = "service";
        var audience = "audience";
        var lifetime = Duration.FromMilliseconds(1);

        var (_, writer, _) = Resolve(
            privateKey,
            signatureAlgorithm,
            issuer,
            audience,
            Duration.FromSeconds(10),
            lifetime
        );
        var encoded = writer.Write(MinimalPrincipal());

        // wait until the token (1 ms lifetime + 1 ms ClockSkew on the reader configured below) is
        // past expiry, so the MS library treats it as expired and the reader maps that to Expired.
        Thread.Sleep(50);

        var (reader, _, _) = Resolve(
            publicKey,
            signatureAlgorithm,
            issuer,
            audience,
            expirationWindow: Duration.FromMilliseconds(1),
            lifetime
        );

        // act
        var result = reader.Read(encoded);

        // assert
        result.Status.Is(TokenReadStatus.Expired);
    }

    /// <summary>
    /// T6.A — <see cref="JwtReadOverrides.ValidateAudience"/> = <c>false</c> must accept a token
    /// whose audience does not match the reader-configured audience. Validates the per-call
    /// override path on top of the otherwise-strict audience check.
    /// </summary>
    /// <param name="privateKey">The private key used for token signing.</param>
    /// <param name="publicKey">The public key used for token validation.</param>
    /// <param name="signatureAlgorithm">The cryptographic algorithm for signing.</param>
    protected void Read_WithAudienceValidationDisabled_AcceptsAudienceMismatch_Base(
        SecurityKey privateKey,
        SecurityKey publicKey,
        string signatureAlgorithm
    )
    {
        // arrange — writer signs with audience-A, reader configured with audience-B
        var time = ResolveTime();
        var writer = new JwtWriter(
            new JwtTokensOptions
            {
                SigningKey = privateKey,
                Algorithm = signatureAlgorithm,
                Issuer = "service",
                Audience = "audience-A",
                Lifetime = Duration.FromSeconds(45),
            },
            time
        );
        var reader = new JwtReader(
            new JwtTokensOptions
            {
                SigningKey = publicKey,
                Algorithm = signatureAlgorithm,
                Issuer = "service",
                Audience = "audience-B",
                ExpirationWindow = Duration.FromSeconds(10),
                Lifetime = Duration.FromSeconds(45),
            },
            time
        );

        var encoded = writer.Write(MinimalPrincipal());

        // sanity — without override the read must fail (audience mismatch)
        reader.Read(encoded).Status.Is(TokenReadStatus.InvalidClaims);

        // act — with the override the read must succeed
        var result = reader.Read(encoded, new JwtReadOverrides(ValidateAudience: false));

        // assert
        result.Status.Is(TokenReadStatus.Ok);
    }

    /// <summary>
    /// T6.A — <see cref="JwtReadOverrides.ValidateLifetime"/> = <c>false</c> must accept an
    /// already-expired token. This is the refresh-token validation use case.
    /// </summary>
    /// <param name="privateKey">The private key used for token signing.</param>
    /// <param name="publicKey">The public key used for token validation.</param>
    /// <param name="signatureAlgorithm">The cryptographic algorithm for signing.</param>
    protected void Read_WithLifetimeValidationDisabled_AcceptsExpiredToken_Base(
        SecurityKey privateKey,
        SecurityKey publicKey,
        string signatureAlgorithm
    )
    {
        // arrange — writer issues a 1-ms-lifetime token; reader has the strict ExpirationWindow
        var time = ResolveTime();
        var lifetime = Duration.FromMilliseconds(1);
        var writer = new JwtWriter(
            new JwtTokensOptions
            {
                SigningKey = privateKey,
                Algorithm = signatureAlgorithm,
                Issuer = "service",
                Audience = "audience",
                Lifetime = lifetime,
            },
            time
        );
        var reader = new JwtReader(
            new JwtTokensOptions
            {
                SigningKey = publicKey,
                Algorithm = signatureAlgorithm,
                Issuer = "service",
                Audience = "audience",
                // tight ClockSkew so the 50ms sleep below is enough to expire the token
                ExpirationWindow = Duration.FromMilliseconds(1),
                Lifetime = lifetime,
            },
            time
        );

        var encoded = writer.Write(MinimalPrincipal());

        // wait long enough that ValidTo + ClockSkew has passed even under concurrent test load
        Thread.Sleep(500);

        // sanity — without override the read must fail (expired)
        reader.Read(encoded).Status.Is(TokenReadStatus.Expired);

        // act — with the override the read must succeed
        var result = reader.Read(encoded, new JwtReadOverrides(ValidateLifetime: false));

        // assert
        result.Status.Is(TokenReadStatus.Ok);
    }

    /// <summary>
    /// T6.A — <see cref="JwtWriteOverrides.Audience"/> must override the configured audience
    /// in the emitted token's <c>aud</c> claim for that single call.
    /// </summary>
    /// <param name="privateKey">The private key used for token signing.</param>
    /// <param name="signatureAlgorithm">The cryptographic algorithm for signing.</param>
    protected void Write_WithAudienceOverride_EmitsAudienceClaim_Base(SecurityKey privateKey, string signatureAlgorithm)
    {
        // arrange — configured audience is "default-aud"; override per-call to "per-call-aud"
        var time = ResolveTime();
        var writer = new JwtWriter(
            new JwtTokensOptions
            {
                SigningKey = privateKey,
                Algorithm = signatureAlgorithm,
                Issuer = "service",
                Audience = "default-aud",
                Lifetime = Duration.FromSeconds(45),
            },
            time
        );

        // act
        var encoded = writer.Write(MinimalPrincipal(), new JwtWriteOverrides(Audience: "per-call-aud"));

        // assert — decoded aud claim is the per-call override
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(encoded);
        jwt.Audiences.Single().Is("per-call-aud");
    }

    /// <summary>
    /// T6.A — <see cref="JwtWriteOverrides.Lifetime"/> must drive the emitted token's
    /// <c>exp - iat</c> for that single call, overriding <see cref="JwtTokensOptions.Lifetime"/>.
    /// </summary>
    /// <param name="privateKey">The private key used for token signing.</param>
    /// <param name="signatureAlgorithm">The cryptographic algorithm for signing.</param>
    protected void Write_WithLifetimeOverride_EmitsCorrectExpClaim_Base(
        SecurityKey privateKey,
        string signatureAlgorithm
    )
    {
        // arrange — configured lifetime 15 minutes; override per-call to 2 hours
        var time = ResolveTime();
        var writer = new JwtWriter(
            new JwtTokensOptions
            {
                SigningKey = privateKey,
                Algorithm = signatureAlgorithm,
                Issuer = "service",
                Audience = "audience",
                Lifetime = Duration.FromMinutes(15),
            },
            time
        );

        // act
        var encoded = writer.Write(MinimalPrincipal(), new JwtWriteOverrides(Lifetime: Duration.FromHours(2)));

        // assert — decoded ValidTo - ValidFrom matches the per-call override (modulo 1s rounding)
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(encoded);
        var span = jwt.ValidTo - jwt.ValidFrom;
        // JWT exp/iat are in seconds since epoch — allow ±1s for floor rounding on either side
        ((int)span.TotalHours).Is(2);
    }

    /// <summary>
    /// Resolves a real-time provider for the override-test scenarios. Mirrors the configuration
    /// used by <see cref="Resolve"/> but returns just the time provider so each test can wire
    /// concrete <see cref="JwtReader"/> / <see cref="JwtWriter"/> instances directly.
    /// </summary>
    /// <returns>The resolved time provider.</returns>
    protected static ITimeProvider ResolveTime()
    {
        var container = new ServiceContainer();
        container.AddTime().WithRealTime().SetDefault();
        return container.BuildServiceProvider().Resolve<ITimeProvider>();
    }

    /// <summary>
    /// Builds the minimal single-claim principal (<c>k=v</c>) used by the scenarios that only need
    /// a token to exist (expiry / audience / lifetime checks), where the specific claims are irrelevant.
    /// </summary>
    /// <returns>A claims principal carrying a single <c>k=v</c> claim.</returns>
    protected static ClaimsPrincipal MinimalPrincipal() =>
        new(new ClaimsIdentity(new[] { new Claim("k", "v") }, "JWT"));
}
