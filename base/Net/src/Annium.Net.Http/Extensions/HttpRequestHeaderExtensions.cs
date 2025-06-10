using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Annium.Net.Http.Extensions;

/// <summary>
/// Extension methods for setting HTTP request headers
/// </summary>
public static class HttpRequestHeaderExtensions
{
    /// <summary>
    /// Sets Basic authentication header for the HTTP request
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="user">The username</param>
    /// <param name="pass">The password</param>
    /// <returns>The HTTP request with Basic authentication header</returns>
    public static IHttpRequest BasicAuthorization(this IHttpRequest request, string user, string pass) =>
        request.Authorization(
            new AuthenticationHeaderValue(
                "Basic",
                $"{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"))}"
            )
        );

    /// <summary>
    /// Sets Bearer authentication header for the HTTP request
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="token">The bearer token</param>
    /// <returns>The HTTP request with Bearer authentication header</returns>
    public static IHttpRequest BearerAuthorization(this IHttpRequest request, string token) =>
        request.Authorization(new AuthenticationHeaderValue("Bearer", token));

    /// <summary>
    /// Adds a cookie to the HTTP request
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="name">The cookie name</param>
    /// <param name="value">The cookie value</param>
    /// <returns>The HTTP request with the cookie added</returns>
    public static IHttpRequest Cookie(this IHttpRequest request, string name, string value) =>
        request.Cookie(new Cookie(name, value));

    /// <summary>
    /// Adds a cookie to the HTTP request
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="cookie">The cookie to add</param>
    /// <returns>The HTTP request with the cookie added</returns>
    public static IHttpRequest Cookie(this IHttpRequest request, Cookie cookie)
    {
        request.Headers.Add("Cookie", cookie.ToString());

        return request;
    }
}
