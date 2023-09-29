using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SimpleBase;

namespace Annium.Identity.Tokens;

public static class KeyReader
{
    public static RsaSecurityKey ReadRsaKey(ReadOnlySpan<char> raw)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(raw);

        return new RsaSecurityKey(rsa)
        {
            KeyId = GetKid(rsa)
        };

        static string GetKid(RSA rsa)
        {
            var publicKeyInfo = rsa.ExportSubjectPublicKeyInfo();
            var kidHash = SHA256.HashData(publicKeyInfo);
            var kidBase32 = Base32.Rfc4648.Encode(kidHash);
            var chunks = new List<string>();
            for (var i = 0; i < 12; i++)
            {
                chunks.Add(kidBase32[(i * 4)..(i * 4 + 4)]);
            }

            return string.Join(':', chunks);
        }
    }
}