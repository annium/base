namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// Represents the status of a JWT read operation
/// </summary>
public enum JwtReadStatus
{
    /// <summary>
    /// The token source is invalid or malformed
    /// </summary>
    BadSource,

    /// <summary>
    /// The token read operation failed
    /// </summary>
    Failed,

    /// <summary>
    /// The token was read successfully
    /// </summary>
    Ok,
}
