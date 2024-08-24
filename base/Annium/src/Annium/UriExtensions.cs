using System;
using System.Runtime.CompilerServices;

namespace Annium;

public static class UriExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Uri EnsureAbsolute(this Uri uri, [CallerArgumentExpression(nameof(uri))] string uriEx = "")
    {
        if (!uri.IsAbsoluteUri)
            throw new InvalidOperationException($"{uriEx} {uri} expected to be absolute uri");

        return uri;
    }
}
