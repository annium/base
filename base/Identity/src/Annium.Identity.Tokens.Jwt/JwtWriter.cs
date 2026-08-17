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

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtWriter"/> class.
    /// </summary>
    /// <param name="options">Token issuing options (issuer, audience, keys, lifetime).</param>
    /// <param name="time">Time provider used to stamp token validity bounds.</param>
    public JwtWriter(JwtTokensOptions options, ITimeProvider time)
    {
        _options = options;
        _time = time;
    }

    /// <summary>
    /// Encodes the given claims as a signed JWT using the configured options without per-call
    /// overrides. Thin call-through to <see cref="Write(ClaimsPrincipal, JwtWriteOverrides?)"/>.
    /// </summary>
    /// <param name="claims">Claims principal whose claims will be added to the JWT payload.</param>
    /// <returns>The encoded JWT.</returns>
    public string Write(ClaimsPrincipal claims) => Write(claims, null);

    /// <summary>
    /// Encodes the given claims as a signed JWT string. Standard claims (<c>iat</c>,
    /// <c>nbf</c>, <c>exp</c>, <c>iss</c>, <c>aud</c>) are populated from the configured
    /// <see cref="JwtTokensOptions"/>; <paramref name="overrides"/> override audience and/or
    /// lifetime per-call. Principal-supplied claims are appended.
    /// </summary>
    /// <param name="claims">Claims principal whose claims will be added to the JWT payload.</param>
    /// <param name="overrides">Optional per-call audience/lifetime overrides; null preserves the no-override path.</param>
    /// <returns>The encoded JWT.</returns>
    public string Write(ClaimsPrincipal claims, JwtWriteOverrides? overrides)
    {
        var now = _time.Now;
        var lifetime = overrides?.Lifetime ?? _options.Lifetime;
        // null/empty audience → omit the aud claim entirely (a null JwtPayload audience adds no claim),
        // honoring JwtTokensOptions.Audience's "null means audience disabled" contract rather than emitting aud="".
        var audience = overrides?.Audience ?? _options.Audience;
        var issuedAt = now.ToDateTimeUtc();
        var expires = (now + lifetime).ToDateTimeUtc();

        var header = new JwtHeader(new SigningCredentials(_options.SigningKey, _options.Algorithm));
        var payload = new JwtPayload(
            _options.Issuer,
            string.IsNullOrEmpty(audience) ? null : audience,
            claims.Claims.ToArray(),
            issuedAt,
            expires,
            issuedAt
        );
        var jwt = new JwtSecurityToken(header, payload);

        return _handler.WriteToken(jwt);
    }
}
