using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Annium.Identity.Tokens.Jwt.Internal;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace Annium.Identity.Tokens.Jwt;

public static class JwtWriter
{
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

    private static JwtHeader CreateHeader(SecurityKey signingKey, string algorithm)
    {
        var header = new JwtHeader(new SigningCredentials(signingKey, algorithm));

        return header;
    }

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
