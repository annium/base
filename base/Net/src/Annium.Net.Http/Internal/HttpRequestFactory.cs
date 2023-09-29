using System;
using Annium.Logging;

namespace Annium.Net.Http.Internal;

internal class HttpRequestFactory : IHttpRequestFactory
{
    private readonly Serializer _httpContentSerializer;
    private readonly ILogger _logger;

    public HttpRequestFactory(
        Serializer httpContentSerializer,
        ILogger logger
    )
    {
        _httpContentSerializer = httpContentSerializer;
        _logger = logger;
    }

    public IHttpRequest New() => new HttpRequest(_httpContentSerializer, _logger);

    public IHttpRequest New(string baseUri) => new HttpRequest(new Uri(baseUri.Trim()), _httpContentSerializer, _logger);

    public IHttpRequest New(Uri baseUri) => new HttpRequest(baseUri, _httpContentSerializer, _logger);
}