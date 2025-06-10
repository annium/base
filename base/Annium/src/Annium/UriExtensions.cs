using System;
using System.Runtime.CompilerServices;

namespace Annium;

/// <summary>
/// Provides extension methods for <see cref="Uri"/> objects.
/// </summary>
public static class UriExtensions
{
    /// <summary>
    /// Ensures that the URI is absolute.
    /// </summary>
    /// <param name="uri">The URI to check.</param>
    /// <param name="uriEx">The expression used to obtain the URI (automatically provided by the compiler).</param>
    /// <returns>The original URI if it is absolute.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the URI is not absolute.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Uri EnsureAbsolute(this Uri uri, [CallerArgumentExpression(nameof(uri))] string uriEx = "")
    {
        if (!uri.IsAbsoluteUri)
            throw new InvalidOperationException($"{uriEx} {uri} expected to be absolute uri");

        return uri;
    }
}
