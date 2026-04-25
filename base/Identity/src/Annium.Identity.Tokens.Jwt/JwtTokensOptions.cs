using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// Configuration for the JWT reader and writer registered via
/// <see cref="ServiceContainerExtensions.AddJwtTokens{TContainer}"/>. Holds the signing key,
/// issuer/audience identity, the validation clock-skew window, and the writer's token lifetime.
/// </summary>
public sealed class JwtTokensOptions
{
    /// <summary>Key used both to sign (writer) and verify (reader) the token.</summary>
    public SecurityKey SigningKey { get; set; } = null!;

    /// <summary>JWT signing algorithm — e.g., <c>SecurityAlgorithms.RsaSha256</c>.</summary>
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>Token issuer (<c>iss</c> claim).</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Optional token audience (<c>aud</c> claim). When null, the audience check is disabled.</summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Reader's clock-skew tolerance for the lifetime check. When null, the reader still
    /// rejects expired / not-yet-valid tokens but with zero slack — see <c>JwtReader.Read</c>.
    /// </summary>
    public Duration? ExpirationWindow { get; set; }

    /// <summary>Lifetime applied by the writer when issuing a new token.</summary>
    public Duration Lifetime { get; set; } = Duration.FromMinutes(15);
}
