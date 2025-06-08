using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Annium.Identity.Tokens;

/// <summary>
/// Extension methods for RSA algorithm operations
/// </summary>
public static class RsaExtensions
{
    /// <summary>
    /// Creates an RSA security key from the algorithm
    /// </summary>
    /// <param name="algorithm">The RSA algorithm</param>
    /// <returns>An RSA security key with generated key ID</returns>
    public static RsaSecurityKey GetKey(this RSA algorithm)
    {
        return new RsaSecurityKey(algorithm) { KeyId = algorithm.GetKeyId() };
    }
}
