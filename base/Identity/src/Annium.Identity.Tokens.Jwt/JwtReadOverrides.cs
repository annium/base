namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// Per-call validation toggles for <see cref="JwtReader.Read(string, JwtReadOverrides?)"/>.
/// Null fields fall back to the behaviour derived from <see cref="JwtTokensOptions"/>.
/// </summary>
/// <param name="ValidateAudience">When non-null, forces audience validation on/off for this call. <c>null</c> falls back to the <see cref="JwtTokensOptions.Audience"/> presence check.</param>
/// <param name="ValidateLifetime">When non-null, forces lifetime validation on/off for this call. <c>null</c> falls back to the <see cref="JwtTokensOptions.ExpirationWindow"/> presence check.</param>
public sealed record JwtReadOverrides(bool? ValidateAudience = null, bool? ValidateLifetime = null);
