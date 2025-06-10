using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using SimpleBase;

namespace Annium.Identity.Tokens;

/// <summary>
/// Extension methods for asymmetric algorithm operations
/// </summary>
public static class AsymmetricAlgorithmExtensions
{
    /// <summary>
    /// Imports a PEM-formatted key into the algorithm
    /// </summary>
    /// <typeparam name="T">The type of asymmetric algorithm</typeparam>
    /// <param name="algorithm">The algorithm instance</param>
    /// <param name="raw">The PEM-formatted key data</param>
    /// <returns>The algorithm instance for method chaining</returns>
    public static T ImportPem<T>(this T algorithm, ReadOnlySpan<char> raw)
        where T : AsymmetricAlgorithm
    {
        algorithm.ImportFromPem(raw);

        return algorithm;
    }

    /// <summary>
    /// Generates a unique key identifier from the algorithm's public key
    /// </summary>
    /// <param name="algorithm">The asymmetric algorithm</param>
    /// <returns>A Base32-encoded key identifier</returns>
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
