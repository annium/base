namespace Annium.Identity.Tokens;

/// <summary>
/// Result of <see cref="ITokenReader{TClaims}.Read"/>. <see cref="Claims"/> is non-null only
/// when <see cref="Status"/> is <see cref="TokenReadStatus.Ok"/>; <see cref="Error"/> carries
/// a human-readable explanation when <see cref="Status"/> is anything else.
/// </summary>
/// <typeparam name="TClaims">Type of the claims principal extracted from a successfully validated token.</typeparam>
/// <param name="Status">Outcome of the read.</param>
/// <param name="Claims">Extracted claims when the read succeeded; null otherwise.</param>
/// <param name="Error">Human-readable error message when the read failed; null on success.</param>
public sealed record TokenReadResult<TClaims>(TokenReadStatus Status, TClaims? Claims, string? Error)
    where TClaims : class;
