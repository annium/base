using NodaTime;

namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// Per-call audience and lifetime overrides for <see cref="JwtWriter.Write(System.Security.Claims.ClaimsPrincipal, JwtWriteOverrides?)"/>.
/// Null fields fall back to <see cref="JwtTokensOptions"/>.
/// </summary>
/// <param name="Audience">Override JWT <c>aud</c> claim for this call. <c>null</c> falls back to <see cref="JwtTokensOptions.Audience"/>.</param>
/// <param name="Lifetime">Override token lifetime for this call (affects <c>exp</c>). <c>null</c> falls back to <see cref="JwtTokensOptions.Lifetime"/>.</param>
public sealed record JwtWriteOverrides(string? Audience = null, Duration? Lifetime = null);
