namespace Annium.Identity.Tokens;

/// <summary>
/// Writes a claims principal as a string-encoded token. Provider-neutral counterpart to
/// <see cref="ITokenReader{TClaims}"/>.
/// </summary>
/// <typeparam name="TClaims">Type of the claims principal to encode.</typeparam>
public interface ITokenWriter<TClaims>
    where TClaims : class
{
    /// <summary>
    /// Encodes the given claims as a token string.
    /// </summary>
    /// <param name="claims">Claims to encode.</param>
    /// <returns>The encoded token.</returns>
    string Write(TClaims claims);
}
