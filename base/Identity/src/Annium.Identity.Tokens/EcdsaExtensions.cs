using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Annium.Identity.Tokens;

public static class EcdsaExtensions
{
    public static ECDsaSecurityKey GetKey(this ECDsa algorithm)
    {
        return new ECDsaSecurityKey(algorithm)
        {
            KeyId = algorithm.GetKeyId()
        };
    }
}