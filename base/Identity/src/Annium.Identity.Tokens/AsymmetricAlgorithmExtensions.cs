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
    /// <summary>Number of groups in a key-id fingerprint (12 × 4 = 240 bits of the SHA-256 hash).</summary>
    private const int KeyIdGroupCount = 12;

    /// <summary>Character width of each key-id fingerprint group.</summary>
    private const int KeyIdGroupSize = 4;

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

        // Fixed-length fingerprint: the 32-byte SHA-256 hash base32-encodes to 52 chars; we take the
        // first 12 groups of 4 (240 bits) and join them as XXXX:...:XXXX. The 12-group format is a
        // deliberate, stable key-id shape (pinned by tests) — 240 bits is far beyond any collision
        // concern, so dropping the final chars is intentional, not truncation of needed entropy.
        for (var i = 0; i < KeyIdGroupCount; i++)
        {
            chunks.Add(kidBase32[(i * KeyIdGroupSize)..(i * KeyIdGroupSize + KeyIdGroupSize)]);
        }

        return string.Join(':', chunks);
    }
}
