using System;
using Annium.Logging;

namespace Annium.Net.Http.Internal;

/// <summary>
/// Factory class for creating HTTP request instances
/// </summary>
internal class HttpRequestFactory : IHttpRequestFactory
{
    /// <summary>
    /// The serializer used for HTTP content serialization
    /// </summary>
    private readonly Serializer _httpContentSerializer;

    /// <summary>
    /// The logger instance for request logging
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the HttpRequestFactory class
    /// </summary>
    /// <param name="httpContentSerializer">The serializer for HTTP content</param>
    /// <param name="logger">The logger instance</param>
    public HttpRequestFactory(Serializer httpContentSerializer, ILogger logger)
    {
        _httpContentSerializer = httpContentSerializer;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new HTTP request instance without a base URI
    /// </summary>
    /// <returns>A new HTTP request instance</returns>
    public IHttpRequest New() => new HttpRequest(_httpContentSerializer, _logger);

    /// <summary>
    /// Creates a new HTTP request instance with a base URI from a string
    /// </summary>
    /// <param name="baseUri">The base URI string</param>
    /// <returns>A new HTTP request instance with the specified base URI</returns>
    public IHttpRequest New(string baseUri) =>
        new HttpRequest(new Uri(baseUri.Trim()), _httpContentSerializer, _logger);

    /// <summary>
    /// Creates a new HTTP request instance with a base URI
    /// </summary>
    /// <param name="baseUri">The base URI</param>
    /// <returns>A new HTTP request instance with the specified base URI</returns>
    public IHttpRequest New(Uri baseUri) => new HttpRequest(baseUri, _httpContentSerializer, _logger);
}
