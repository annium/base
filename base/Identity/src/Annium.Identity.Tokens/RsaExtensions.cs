using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Annium.Identity.Tokens;

public static class RsaExtensions
{
    public static RsaSecurityKey GetKey(this RSA rsa)
    {
        return new RsaSecurityKey(rsa)
        {
            KeyId = rsa.GetKeyId()
        };
    }
}