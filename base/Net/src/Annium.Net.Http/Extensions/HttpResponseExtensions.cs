using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class HttpResponseExtensions
{
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