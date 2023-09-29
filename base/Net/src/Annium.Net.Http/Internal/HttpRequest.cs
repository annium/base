using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Base;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Http.Internal;

internal class HttpRequest : IHttpRequest
{
    private delegate void Configuration(IHttpRequest request);

    private delegate Task<IHttpResponse> Middleware(Func<Task<IHttpResponse>> next, IHttpRequest request);

    private static readonly HttpClient DefaultClient = new();

    public HttpMethod Method { get; private set; } = HttpMethod.Get;
    public Uri Uri => Helper.BuildUri(_client, _baseUri, _uri, _parameters);
    public Uri Path => Helper.GetUriFactory(_client, _baseUri, _uri).Build();
    public HttpRequestHeaders Headers { get; }
    public IReadOnlyDictionary<string, StringValues> Params => _parameters;
    public HttpContent? Content { get; private set; }
    public Serializer Serializer { get; }
    public ILogger Logger { get; }
    private HttpClient _client = DefaultClient;
    private Uri? _baseUri;
    private string? _uri;
    private TimeSpan _timeout = TimeSpan.FromSeconds(30);
    private readonly Dictionary<string, StringValues> _parameters = new();
    private readonly List<Configuration> _configurations = new();
    private readonly List<Middleware> _middlewares = new();

    internal HttpRequest(
        Uri baseUri,
        Serializer httpContentSerializer,
        ILogger logger
    ) : this(
        httpContentSerializer,
        logger
    )
    {
        Logger = logger;
        _baseUri = baseUri;
    }

    internal HttpRequest(
        Serializer httpContentSerializer,
        ILogger logger
    )
    {
        Serializer = httpContentSerializer;
        Logger = logger;
        using var message = new HttpRequestMessage();
        Headers = message.Headers;
    }

    public IHttpRequest Base(Uri baseUri)
    {
        _baseUri = baseUri;

        return this;
    }

    public IHttpRequest Base(string baseUri) => Base(new Uri(baseUri));

    public IHttpRequest UseClient(HttpClient client)
    {
        _client = client;

        return this;
    }

    public IHttpRequest With(HttpMethod method, Uri uri) => With(method, uri.ToString());

    public IHttpRequest With(HttpMethod method, string uri)
    {
        Method = method;
        _uri = uri;

        return this;
    }

    public IHttpRequest Header(string name, string value)
    {
        Headers.Add(name, value);

        return this;
    }

    public IHttpRequest Header(string name, IEnumerable<string> values)
    {
        Headers.Add(name, values);

        return this;
    }

    public IHttpRequest Authorization(AuthenticationHeaderValue value)
    {
        Headers.Authorization = value;

        return this;
    }

    public IHttpRequest Param<T>(string key, T value)
    {
        _parameters[key] = value?.ToString() ?? string.Empty;

        return this;
    }

    public IHttpRequest Param<T>(string key, List<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, IList<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, IReadOnlyList<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, IReadOnlyCollection<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, T[] values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, IEnumerable<T> values)
    {
        var parameters =
        (
            from value in values
            where value is not null
            select value.ToString()
        ).ToArray();

        _parameters[key] = new StringValues(parameters);

        return this;
    }

    public IHttpRequest Attach(HttpContent content)
    {
        Content = content;

        return this;
    }

    public IHttpRequest Timeout(TimeSpan timeout)
    {
        _timeout = timeout;

        return this;
    }

    public IHttpRequest Configure(Action<IHttpRequest> configure)
    {
        _configurations.Add(new Configuration(configure));

        return this;
    }

    public IHttpRequest Intercept(Func<Func<Task<IHttpResponse>>, Task<IHttpResponse>> middleware)
    {
        _middlewares.Add((next, _) => middleware(next));

        return this;
    }

    public IHttpRequest Intercept(Func<Func<Task<IHttpResponse>>, IHttpRequest, Task<IHttpResponse>> middleware)
    {
        _middlewares.Add(new Middleware(middleware));

        return this;
    }

    public async Task<IHttpResponse> RunAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        var cts = CancellationTokenSource.CreateLinkedTokenSource(
            new CancellationTokenSource(_timeout).Token,
            ct
        );

        foreach (var configure in _configurations)
            configure(this);

        var response = _middlewares.Count > 0
            ? await InternalRunAsync(0, cts.Token).ConfigureAwait(false)
            : await InternalRunAsync(cts.Token).ConfigureAwait(false);

        this.Trace("done");

        return response;
    }

    private async Task<IHttpResponse> InternalRunAsync(
        int middlewareIndex,
        CancellationToken ct
    )
    {
        this.Trace("start {index}/{total}", middlewareIndex + 1, _middlewares.Count);

        if (ct.IsCancellationRequested)
        {
            this.Trace("canceled");
            return new HttpResponse(Uri, HttpStatusCode.RequestTimeout, "Request canceled", string.Empty);
        }

        var middleware = _middlewares[middlewareIndex];
        Func<Task<IHttpResponse>> next = middlewareIndex + 1 < _middlewares.Count
            ? () => InternalRunAsync(middlewareIndex + 1, ct)
            : () => InternalRunAsync(ct);

        var response = await middleware(next, this).ConfigureAwait(false);

        this.Trace("done {index}/{total}", middlewareIndex + 1, _middlewares.Count);

        return response;
    }

    private async Task<IHttpResponse> InternalRunAsync(CancellationToken ct)
    {
        this.Trace("start");

        if (ct.IsCancellationRequested)
        {
            this.Trace("canceled");
            return new HttpResponse(Uri, HttpStatusCode.RequestTimeout, "Request canceled", string.Empty);
        }

        var uri = Uri;
        var requestMessage = new HttpRequestMessage { Method = Method, RequestUri = uri };

        foreach (var (name, values) in Headers)
            requestMessage.Headers.Add(name, values);

        requestMessage.Content = Content;

        try
        {
            this.Trace("send request");
            var responseMessage = await _client.SendAsync(requestMessage, ct).ConfigureAwait(false);

            this.Trace("prepare response");
            var response = new HttpResponse(uri, responseMessage);

            this.Trace("done");

            return response;
        }
        catch (HttpRequestException e)
        {
            this.Trace("handle connection refused");
            return new HttpResponse(uri, HttpStatusCode.ServiceUnavailable, "Connection refused", e.Message);
        }
        catch (OperationCanceledException e)
        {
            this.Trace("handle task canceled");
            return new HttpResponse(uri, HttpStatusCode.RequestTimeout, "Request canceled", e.Message);
        }
    }
}

file static class Helper
{
    public static Uri BuildUri(
        HttpClient client,
        Uri? baseUri,
        string? uri,
        IReadOnlyDictionary<string, StringValues> parameters
    )
    {
        var factory = GetUriFactory(client, baseUri, uri);

        // add manually defined params to queryBuilder
        foreach (var (name, value) in parameters)
            factory.Param(name, value);

        return factory.Build();
    }

    public static UriFactory GetUriFactory(
        HttpClient client,
        Uri? baseUri,
        string? uri
    )
    {
        var baseAddress = baseUri ?? client.BaseAddress;
        if (baseAddress is null)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("Request URI is empty");

            return UriFactory.Base(uri);
        }

        if (string.IsNullOrWhiteSpace(uri))
            return UriFactory.Base(baseAddress);

        return UriFactory.Base(baseAddress).Path(uri);
    }
}