using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Annium.Identity.Tokens;

public static class RsaExtensions
{
    public static RsaSecurityKey GetKey(this RSA algorithm)
    {
        return new RsaSecurityKey(algorithm) { KeyId = algorithm.GetKeyId() };
    }
}
