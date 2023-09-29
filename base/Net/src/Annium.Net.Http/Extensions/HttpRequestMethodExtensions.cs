using System;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class HttpRequestMethodExtensions
{
    public static IHttpRequest Head(this IHttpRequest request, string url) => request.With(HttpMethod.Head, url);

    public static IHttpRequest Head(this IHttpRequest request, Uri url) => request.With(HttpMethod.Head, url);

    public static IHttpRequest Get(this IHttpRequest request, string url) => request.With(HttpMethod.Get, url);

    public static IHttpRequest Get(this IHttpRequest request, Uri url) => request.With(HttpMethod.Get, url);

    public static IHttpRequest Put(this IHttpRequest request, string url) => request.With(HttpMethod.Put, url);

    public static IHttpRequest Put(this IHttpRequest request, Uri url) => request.With(HttpMethod.Put, url);

    public static IHttpRequest Post(this IHttpRequest request, string url) => request.With(HttpMethod.Post, url);

    public static IHttpRequest Post(this IHttpRequest request, Uri url) => request.With(HttpMethod.Post, url);

    public static IHttpRequest Delete(this IHttpRequest request, string url) => request.With(HttpMethod.Delete, url);

    public static IHttpRequest Delete(this IHttpRequest request, Uri url) => request.With(HttpMethod.Delete, url);
}