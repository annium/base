using System;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

/// <summary>
/// Extension methods for setting HTTP request methods and URLs
/// </summary>
public static class HttpRequestMethodExtensions
{
    /// <summary>
    /// Sets the HTTP method to HEAD and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with HEAD method and URL set</returns>
    public static IHttpRequest Head(this IHttpRequest request, string url) => request.With(HttpMethod.Head, url);

    /// <summary>
    /// Sets the HTTP method to HEAD and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with HEAD method and URL set</returns>
    public static IHttpRequest Head(this IHttpRequest request, Uri url) => request.With(HttpMethod.Head, url);

    /// <summary>
    /// Sets the HTTP method to GET and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with GET method and URL set</returns>
    public static IHttpRequest Get(this IHttpRequest request, string url) => request.With(HttpMethod.Get, url);

    /// <summary>
    /// Sets the HTTP method to GET and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with GET method and URL set</returns>
    public static IHttpRequest Get(this IHttpRequest request, Uri url) => request.With(HttpMethod.Get, url);

    /// <summary>
    /// Sets the HTTP method to PUT and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with PUT method and URL set</returns>
    public static IHttpRequest Put(this IHttpRequest request, string url) => request.With(HttpMethod.Put, url);

    /// <summary>
    /// Sets the HTTP method to PUT and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with PUT method and URL set</returns>
    public static IHttpRequest Put(this IHttpRequest request, Uri url) => request.With(HttpMethod.Put, url);

    /// <summary>
    /// Sets the HTTP method to POST and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with POST method and URL set</returns>
    public static IHttpRequest Post(this IHttpRequest request, string url) => request.With(HttpMethod.Post, url);

    /// <summary>
    /// Sets the HTTP method to POST and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with POST method and URL set</returns>
    public static IHttpRequest Post(this IHttpRequest request, Uri url) => request.With(HttpMethod.Post, url);

    /// <summary>
    /// Sets the HTTP method to DELETE and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with DELETE method and URL set</returns>
    public static IHttpRequest Delete(this IHttpRequest request, string url) => request.With(HttpMethod.Delete, url);

    /// <summary>
    /// Sets the HTTP method to DELETE and the URL
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="url">The URL to request</param>
    /// <returns>The HTTP request with DELETE method and URL set</returns>
    public static IHttpRequest Delete(this IHttpRequest request, Uri url) => request.With(HttpMethod.Delete, url);
}
