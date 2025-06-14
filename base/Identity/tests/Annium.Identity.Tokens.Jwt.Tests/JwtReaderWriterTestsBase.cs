using System;
using System.Linq;
using Annium.NodaTime.Extensions;
using Annium.Testing;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace Annium.Identity.Tokens.Jwt.Tests;

/// <summary>
/// Base class providing common test functionality for JWT reader and writer operations.
/// Contains shared test logic for validating JWT token creation, encoding, and validation.
/// </summary>
public class JwtReaderWriterTestsBase
{
    /// <summary>
    /// Base test method that validates JWT token creation and reading with specified cryptographic keys.
    /// Tests the complete round-trip of token creation, encoding, and validation.
    /// </summary>
    /// <param name="privateKey">The private key used for token signing</param>
    /// <param name="publicKey">The public key used for token validation</param>
    /// <param name="signatureAlgorithm">The cryptographic algorithm for signing</param>
    protected void Works_Base(SecurityKey privateKey, SecurityKey publicKey, string signatureAlgorithm)
    {
        // arrange
        var tokenId = Guid.NewGuid().ToString();
        var issuer = "service";
        var audience = "audience";
        var now = SystemClock.Instance.GetCurrentInstant().FloorToSecond();
        var nowUtc = now.ToDateTimeUtc();
        var lifetime = Duration.FromSeconds(45);
        var expiresUtc = (now + lifetime).ToDateTimeUtc();
        var key = "sample";
        var data = "g87asgdf";
        var opts = JwtReader.GetValidationParameters(publicKey, issuer, audience, Duration.FromSeconds(10));

        // act - write
        var token = JwtWriter.Create(
            privateKey,
            signatureAlgorithm,
            tokenId,
            issuer,
            audience,
            now,
            lifetime,
            (key, data)
        );
        var encoded = token.GetString();

        // assert - write
        token.IsNotDefault();
        token.Id.Is(tokenId);
        token.Issuer.Is(issuer);
        token.Audiences.Has(1);
        token.Audiences.At(0).Is(audience);
        token.IssuedAt.Is(nowUtc);
        token.ValidFrom.Is(nowUtc);
        token.ValidTo.Is(expiresUtc);
        token.Claims.FirstOrDefault(x => x.Type == key).IsNotDefault().Value.Is(data);

        // act - read
        var readResult = JwtReader.Read(encoded, opts, now);

        // assert - read
        readResult.HasErrors.IsFalse();
        var (status, restored) = readResult;
        status.Is(JwtReadStatus.Ok);
        restored.IsT0.IsTrue();
        restored.AsT0.IsNotDefault();
        restored.AsT0.Id.Is(tokenId);
        restored.AsT0.Issuer.Is(issuer);
        restored.AsT0.Audiences.Has(1);
        restored.AsT0.Audiences.At(0).Is(audience);
        restored.AsT0.IssuedAt.Is(nowUtc);
        restored.AsT0.ValidFrom.Is(nowUtc);
        restored.AsT0.ValidTo.Is(expiresUtc);
        restored.AsT0.Claims.FirstOrDefault(x => x.Type == key).IsNotDefault().Value.Is(data);
    }
}
