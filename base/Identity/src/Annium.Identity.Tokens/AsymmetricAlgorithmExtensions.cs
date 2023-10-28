using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using SimpleBase;

namespace Annium.Identity.Tokens;

public static class AsymmetricAlgorithmExtensions
{
    public static T ImportPem<T>(this T algorithm, ReadOnlySpan<char> raw)
        where T : AsymmetricAlgorithm
    {
        algorithm.ImportFromPem(raw);

        return algorithm;
    }

    public static string GetKeyId(this AsymmetricAlgorithm algorithm)
    {
        var publicKeyInfo = algorithm.ExportSubjectPublicKeyInfo();
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
