using System;

namespace Annium.Net.Http;

/// <summary>
/// Factory for creating HTTP request instances
/// </summary>
public interface IHttpRequestFactory
{
    /// <summary>
    /// Creates a new HTTP request
    /// </summary>
    /// <returns>A new HTTP request instance</returns>
    IHttpRequest New();

    /// <summary>
    /// Creates a new HTTP request with a base URI
    /// </summary>
    /// <param name="baseUri">The base URI string</param>
    /// <returns>A new HTTP request instance</returns>
    IHttpRequest New(string baseUri);

    /// <summary>
    /// Creates a new HTTP request with a base URI
    /// </summary>
    /// <param name="baseUri">The base URI</param>
    /// <returns>A new HTTP request instance</returns>
    IHttpRequest New(Uri baseUri);
}
