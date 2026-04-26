namespace Annium.Identity.Tokens;

/// <summary>
/// Reads and validates a string-encoded token, producing a strongly typed claims principal
/// or a typed failure status. Provider-neutral — JWT, opaque, or other token formats all
/// fit behind this contract.
/// </summary>
/// <typeparam name="TClaims">Type of the claims principal extracted from a successfully validated token.</typeparam>
public interface ITokenReader<TClaims>
    where TClaims : class
{
    /// <summary>
    /// Reads and validates the token. Returns a result with <see cref="TokenReadStatus.Ok"/>
    /// on success (with <see cref="TokenReadResult{TClaims}.Claims"/> populated), or one of
    /// the failure statuses with an error message.
    /// </summary>
    /// <param name="token">String-encoded token to read.</param>
    /// <returns>The read result.</returns>
    TokenReadResult<TClaims> Read(string token);
}
