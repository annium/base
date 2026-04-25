using System;
using System.Linq;
using System.Security.Claims;
using Annium;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Identity.Tokens;
using Annium.NodaTime.Extensions;
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
    private static (ITokenReader<ClaimsPrincipal> reader, ITokenWriter<ClaimsPrincipal> writer, ITimeProvider time) Resolve(
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
        return (sp.Resolve<ITokenReader<ClaimsPrincipal>>(), sp.Resolve<ITokenWriter<ClaimsPrincipal>>(), sp.Resolve<ITimeProvider>());
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

        var (_, writer, _) = Resolve(privateKey, signatureAlgorithm, issuer, audience, Duration.FromSeconds(10), lifetime);
        var (reader, _, _) = Resolve(publicKey, signatureAlgorithm, issuer, audience, Duration.FromSeconds(10), lifetime);

        var inputClaims = new ClaimsIdentity(new[] { new Claim("jti", tokenId), new Claim(key, data) }, "JWT");
        var inputPrincipal = new ClaimsPrincipal(inputClaims);

        // act - write
        var encoded = writer.Write(inputPrincipal);

        // assert - encoded is non-empty
        encoded.IsNotDefault();
        encoded.Length.Is(encoded.Length); // sanity (used to silence unused-var)

        // act - read
        var result = reader.Read(encoded);

        // assert - read
        result.Status.Is(TokenReadStatus.Ok);
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
        // arrange — write a token with a 1-hour past lifetime
        var issuer = "service";
        var audience = "audience";
        var lifetime = Duration.FromMilliseconds(1); // immediately expires

        var (_, writer, _) = Resolve(privateKey, signatureAlgorithm, issuer, audience, Duration.FromSeconds(10), lifetime);
        var encoded = writer.Write(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("k", "v") }, "JWT")));

        // wait so the token's ValidTo (now + 1ms) is in the past
        System.Threading.Thread.Sleep(50);

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

        var (_, writer, _) = Resolve(privateKey, signatureAlgorithm, issuer, audience, Duration.FromSeconds(10), lifetime);
        var encoded = writer.Write(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("k", "v") }, "JWT")));

        // wait so the token expires (lifetime 1ms) plus ClockSkew (10s applied below) — need
        // to wait > 10s for the MS library to treat it as expired. We use a tighter
        // ClockSkew here (1ms) so the test stays fast.
        System.Threading.Thread.Sleep(50);

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
}
