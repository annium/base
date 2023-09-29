using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class HttpRequestHeaderExtensions
{
    public static IHttpRequest BasicAuthorization(this IHttpRequest request, string user, string pass) =>
        request.Authorization(new AuthenticationHeaderValue("Basic", $"{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"))}"));

    public static IHttpRequest BearerAuthorization(this IHttpRequest request, string token) =>
        request.Authorization(new AuthenticationHeaderValue("Bearer", token));

    public static IHttpRequest Cookie(this IHttpRequest request, string name, string value) =>
        request.Cookie(new Cookie(name, value));

    public static IHttpRequest Cookie(this IHttpRequest request, Cookie cookie)
    {
        request.Headers.Add("Cookie", cookie.ToString());

        return request;
    }
}