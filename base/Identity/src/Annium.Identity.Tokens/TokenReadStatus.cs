namespace Annium.Identity.Tokens;

/// <summary>
/// Outcome of reading and validating a token. Provider-neutral — a JWT-specific reader
/// (<c>Annium.Identity.Tokens.Jwt.JwtReader</c>) maps platform-specific exceptions to these
/// statuses; future readers (e.g., for opaque tokens) populate the same surface.
/// </summary>
public enum TokenReadStatus
{
    /// <summary>Token validated successfully.</summary>
    Ok,

    /// <summary>Token's expiration moment is in the past.</summary>
    Expired,

    /// <summary>Token's "valid from" moment is in the future.</summary>
    NotYetValid,

    /// <summary>Signature could not be verified — wrong key, tampered payload, or unsupported algorithm.</summary>
    InvalidSignature,

    /// <summary>Issuer, audience, or other claim-level validation rejected the token.</summary>
    InvalidClaims,

    /// <summary>Token text is not parseable as the expected format.</summary>
    Malformed,

    /// <summary>Validation failed for a reason not classified above.</summary>
    Unknown,
}
