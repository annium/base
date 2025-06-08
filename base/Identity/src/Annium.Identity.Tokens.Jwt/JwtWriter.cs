using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Annium.Identity.Tokens.Jwt.Internal;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// Provides functionality for creating JWT tokens
/// </summary>
public static class JwtWriter
{
    /// <summary>
    /// Creates a new JWT security token
    /// </summary>
    /// <param name="securityKey">The security key for signing</param>
    /// <param name="algorithm">The signing algorithm</param>
    /// <param name="tokenId">The unique token identifier</param>
    /// <param name="issuer">The token issuer</param>
    /// <param name="audience">The token audience</param>
    /// <param name="now">The current time</param>
    /// <param name="lifetime">The token lifetime</param>
    /// <param name="data">Additional claims data</param>
    /// <returns>The created JWT security token</returns>
    public static JwtSecurityToken Create(
        SecurityKey securityKey,
        string algorithm,
        string tokenId,
        string issuer,
        string audience,
        Instant now,
        Duration lifetime,
        params (string key, string value)[] data
    )
    {
        var header = CreateHeader(securityKey, algorithm);
        var payload = CreatePayload(tokenId, issuer, audience, now, lifetime, data);
        var jwt = new JwtSecurityToken(header, payload);

        return jwt;
    }

    /// <summary>
    /// Creates the JWT header with signing credentials
    /// </summary>
    /// <param name="signingKey">The security key used for signing</param>
    /// <param name="algorithm">The signing algorithm to use</param>
    /// <returns>The JWT header with configured signing credentials</returns>
    private static JwtHeader CreateHeader(SecurityKey signingKey, string algorithm)
    {
        var header = new JwtHeader(new SigningCredentials(signingKey, algorithm));

        return header;
    }

    /// <summary>
    /// Creates the JWT payload with standard and custom claims
    /// </summary>
    /// <param name="tokenId">The unique token identifier</param>
    /// <param name="issuer">The token issuer</param>
    /// <param name="audience">The token audience</param>
    /// <param name="now">The current time for issued at claim</param>
    /// <param name="lifetime">The token lifetime for expiration</param>
    /// <param name="data">Additional custom claims as key-value pairs</param>
    /// <returns>The JWT payload with all configured claims</returns>
    private static JwtPayload CreatePayload(
        string tokenId,
        string issuer,
        string audience,
        Instant now,
        Duration lifetime,
        params (string key, string value)[] data
    )
    {
        var issuedAt = now.ToDateTimeUtc();
        var expires = (now + lifetime).ToDateTimeUtc();

        var claims = new Claim[] { new(Claims.TokenId, tokenId) }
            .Concat(data.Select(x => new Claim(x.key, x.value)))
            .ToArray();

        var payload = new JwtPayload(issuer, audience, claims, issuedAt, expires, issuedAt);

        return payload;
    }
}
