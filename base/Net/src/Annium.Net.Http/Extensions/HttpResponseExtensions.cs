using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Annium.Net.Http.Extensions;

/// <summary>
/// Extension methods for HTTP response processing
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Extracts cookies from the HTTP response
    /// </summary>
    /// <param name="response">The HTTP response</param>
    /// <returns>A collection of cookies from the response</returns>
    public static IReadOnlyCollection<Cookie> Cookies(this IHttpResponse response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
            return Array.Empty<Cookie>();

        var uri = new Uri(response.Uri.GetLeftPart(UriPartial.Authority));

        var cookies = new CookieContainer();
        foreach (var cookieHeader in cookieHeaders)
            cookies.SetCookies(uri, cookieHeader);

        return cookies.GetCookies(uri).ToArray();
    }
}
