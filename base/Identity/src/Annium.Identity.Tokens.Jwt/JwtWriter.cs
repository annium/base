using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// JWT-backed implementation of <see cref="ITokenWriter{ClaimsPrincipal}"/>. Configured via
/// <see cref="JwtTokensOptions"/>; current time supplied by <see cref="ITimeProvider"/> for
/// the <c>iat</c>/<c>nbf</c>/<c>exp</c> claims.
/// </summary>
public sealed class JwtWriter : ITokenWriter<ClaimsPrincipal>
{
    /// <summary>Writer configuration — signing key, algorithm, issuer, audience, lifetime.</summary>
    private readonly JwtTokensOptions _options;

    /// <summary>Time provider supplying the issued-at moment.</summary>
    private readonly ITimeProvider _time;

    /// <summary>Per-instance handler — <c>JwtSecurityTokenHandler.WriteToken(SecurityToken)</c> is thread-safe.</summary>
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtWriter(JwtTokensOptions options, ITimeProvider time)
    {
        _options = options;
        _time = time;
    }

    /// <summary>
    /// Encodes the given claims as a signed JWT string. Standard claims (<c>iat</c>,
    /// <c>nbf</c>, <c>exp</c>, <c>iss</c>, <c>aud</c>) are populated from the configured
    /// <see cref="JwtTokensOptions"/>; principal-supplied claims are appended.
    /// </summary>
    /// <param name="claims">Claims principal whose claims will be added to the JWT payload.</param>
    /// <returns>The encoded JWT.</returns>
    public string Write(ClaimsPrincipal claims)
    {
        var now = _time.Now;
        var issuedAt = now.ToDateTimeUtc();
        var expires = (now + _options.Lifetime).ToDateTimeUtc();

        var header = new JwtHeader(new SigningCredentials(_options.SigningKey, _options.Algorithm));
        var payload = new JwtPayload(
            _options.Issuer,
            _options.Audience ?? string.Empty,
            claims.Claims.ToArray(),
            issuedAt,
            expires,
            issuedAt
        );
        var jwt = new JwtSecurityToken(header, payload);

        return _handler.WriteToken(jwt);
    }
}
