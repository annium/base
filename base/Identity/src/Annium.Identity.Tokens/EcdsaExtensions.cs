using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Annium.Identity.Tokens;

/// <summary>
/// Extension methods for ECDSA algorithm operations
/// </summary>
public static class EcdsaExtensions
{
    /// <summary>
    /// Creates an ECDSA security key from the algorithm
    /// </summary>
    /// <param name="algorithm">The ECDSA algorithm</param>
    /// <returns>An ECDSA security key with generated key ID</returns>
    public static ECDsaSecurityKey GetKey(this ECDsa algorithm)
    {
        return new ECDsaSecurityKey(algorithm) { KeyId = algorithm.GetKeyId() };
    }
}
