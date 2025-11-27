using System;
using System.Net.Http;
using Annium.Logging;

namespace Annium.Net.Http.Internal;

/// <summary>
/// Factory class for creating HTTP request instances
/// </summary>
internal class HttpRequestFactory : IHttpRequestFactory
{
    /// <summary>
    /// HttpClient, bound to factory. Is disposed with container, factory is created within
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// The serializer used for HTTP content serialization
    /// </summary>
    private readonly Serializer _contentSerializer;

    /// <summary>
    /// The logger instance for request logging
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the HttpRequestFactory class
    /// </summary>
    /// <param name="client">The HttpClient to handle requests with</param>
    /// <param name="contentSerializer">The serializer for HTTP content</param>
    /// <param name="logger">The logger instance</param>
    public HttpRequestFactory(HttpClient client, Serializer contentSerializer, ILogger logger)
    {
        _client = client;
        _contentSerializer = contentSerializer;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new HTTP request instance without a base URI
    /// </summary>
    /// <returns>A new HTTP request instance</returns>
    public IHttpRequest New() => new HttpRequest(_client, _contentSerializer, _logger);

    /// <summary>
    /// Creates a new HTTP request instance with a base URI from a string
    /// </summary>
    /// <param name="baseUri">The base URI string</param>
    /// <returns>A new HTTP request instance with the specified base URI</returns>
    public IHttpRequest New(string baseUri) =>
        new HttpRequest(_client, new Uri(baseUri.Trim()), _contentSerializer, _logger);

    /// <summary>
    /// Creates a new HTTP request instance with a base URI
    /// </summary>
    /// <param name="baseUri">The base URI</param>
    /// <returns>A new HTTP request instance with the specified base URI</returns>
    public IHttpRequest New(Uri baseUri) => new HttpRequest(_client, baseUri, _contentSerializer, _logger);
}
