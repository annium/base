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

/// <summary>
/// Internal implementation of IHttpRequest that provides HTTP request functionality with middleware support
/// </summary>
internal class HttpRequest : IHttpRequest
{
    /// <summary>
    /// Delegate type for request configuration actions
    /// </summary>
    /// <param name="request">The HTTP request to configure</param>
    private delegate void Configuration(IHttpRequest request);

    /// <summary>
    /// Delegate type for middleware functions that can intercept and modify HTTP requests
    /// </summary>
    /// <param name="next">The next middleware or final request handler</param>
    /// <param name="request">The HTTP request being processed</param>
    /// <returns>The HTTP response</returns>
    private delegate Task<IHttpResponse> Middleware(Func<Task<IHttpResponse>> next, IHttpRequest request);

    /// <summary>
    /// Gets or sets the HTTP method for the request
    /// </summary>
    public HttpMethod Method { get; private set; } = HttpMethod.Get;

    /// <summary>
    /// Gets the complete URI for the request including base URI, path, and query parameters
    /// </summary>
    public Uri Uri => Helper.BuildUri(_client, _baseUri, _uri, _parameters);

    /// <summary>
    /// Gets the path portion of the URI without query parameters
    /// </summary>
    public Uri Path => Helper.GetUriFactory(_client, _baseUri, _uri).Build();

    /// <summary>
    /// Gets the HTTP request headers
    /// </summary>
    public HttpRequestHeaders Headers { get; }

    /// <summary>
    /// Gets the query parameters for the request
    /// </summary>
    public IReadOnlyDictionary<string, StringValues> Params => _parameters;

    /// <summary>
    /// Gets or sets the HTTP content for the request body
    /// </summary>
    public HttpContent? Content { get; private set; }

    /// <summary>
    /// Gets the serializer used for content serialization/deserialization
    /// </summary>
    public Serializer Serializer { get; }

    /// <summary>
    /// Gets the logger instance for this request
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The HTTP client to use for sending the request
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// The base URI for the request
    /// </summary>
    private Uri? _baseUri;

    /// <summary>
    /// The relative URI path for the request
    /// </summary>
    private string? _uri;

    /// <summary>
    /// The timeout duration for the request
    /// </summary>
    private TimeSpan _timeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Collection of query parameters for the request
    /// </summary>
    private readonly Dictionary<string, StringValues> _parameters = new();

    /// <summary>
    /// Collection of configuration actions to apply before request execution
    /// </summary>
    private readonly List<Configuration> _configurations = new();

    /// <summary>
    /// Collection of middleware functions to apply during request execution
    /// </summary>
    private readonly List<Middleware> _middlewares = new();

    /// <summary>
    /// Initializes a new instance of the HttpRequest class with a base URI
    /// </summary>
    /// <param name="client">The HttpClient to handle request with</param>
    /// <param name="baseUri">The base URI for the request</param>
    /// <param name="httpContentSerializer">The serializer for HTTP content</param>
    /// <param name="logger">The logger instance</param>
    internal HttpRequest(HttpClient client, Uri baseUri, Serializer httpContentSerializer, ILogger logger)
        : this(client, httpContentSerializer, logger)
    {
        _baseUri = baseUri.NotNull();
    }

    /// <summary>
    /// Initializes a new instance of the HttpRequest class
    /// </summary>
    /// <param name="client">The HttpClient to handle request with</param>
    /// <param name="httpContentSerializer">The serializer for HTTP content</param>
    /// <param name="logger">The logger instance</param>
    internal HttpRequest(HttpClient client, Serializer httpContentSerializer, ILogger logger)
    {
        _client = client;
        Serializer = httpContentSerializer;
        Logger = logger;
        using var message = new HttpRequestMessage();
        Headers = message.Headers;
    }

    /// <summary>
    /// Sets the base URI for the request
    /// </summary>
    /// <param name="baseUri">The base URI to set</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Base(Uri baseUri)
    {
        _baseUri = baseUri.NotNull();

        return this;
    }

    /// <summary>
    /// Sets the base URI for the request from a string
    /// </summary>
    /// <param name="baseUri">The base URI string to set</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Base(string baseUri) => Base(new Uri(baseUri));

    /// <summary>
    /// Sets the HTTP method and URI for the request
    /// </summary>
    /// <param name="method">The HTTP method to use</param>
    /// <param name="uri">The URI for the request</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest With(HttpMethod method, Uri uri) => With(method, uri.ToString());

    /// <summary>
    /// Sets the HTTP method and URI for the request
    /// </summary>
    /// <param name="method">The HTTP method to use</param>
    /// <param name="uri">The URI string for the request</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest With(HttpMethod method, string uri)
    {
        Method = method;
        _uri = uri.NotNull();

        return this;
    }

    /// <summary>
    /// Adds a header to the request
    /// </summary>
    /// <param name="name">The header name</param>
    /// <param name="value">The header value</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Header(string name, string value)
    {
        Headers.Add(name.NotNull(), value.NotNull());

        return this;
    }

    /// <summary>
    /// Adds a header with multiple values to the request
    /// </summary>
    /// <param name="name">The header name</param>
    /// <param name="values">The header values</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Header(string name, IEnumerable<string> values)
    {
        Headers.Add(name.NotNull(), values.NotNull());

        return this;
    }

    /// <summary>
    /// Sets the authorization header for the request
    /// </summary>
    /// <param name="value">The authentication header value</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Authorization(AuthenticationHeaderValue value)
    {
        Headers.Authorization = value.NotNull();

        return this;
    }

    /// <summary>
    /// Adds a query parameter to the request
    /// </summary>
    /// <typeparam name="T">The type of the parameter value</typeparam>
    /// <param name="key">The parameter key</param>
    /// <param name="value">The parameter value</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Param<T>(string key, T value)
    {
        _parameters[key.NotNull()] = value?.ToString() ?? string.Empty;

        return this;
    }

    /// <summary>
    /// Adds a query parameter with multiple values from a List
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter key</param>
    /// <param name="values">The parameter values</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Param<T>(string key, List<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    /// <summary>
    /// Adds a query parameter with multiple values from an IList
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter key</param>
    /// <param name="values">The parameter values</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Param<T>(string key, IList<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    /// <summary>
    /// Adds a query parameter with multiple values from an IReadOnlyList
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter key</param>
    /// <param name="values">The parameter values</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Param<T>(string key, IReadOnlyList<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    /// <summary>
    /// Adds a query parameter with multiple values from an IReadOnlyCollection
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter key</param>
    /// <param name="values">The parameter values</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Param<T>(string key, IReadOnlyCollection<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    /// <summary>
    /// Adds a query parameter with multiple values from an array
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter key</param>
    /// <param name="values">The parameter values</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Param<T>(string key, T[] values)
    {
        return Param(key, values.AsEnumerable());
    }

    /// <summary>
    /// Adds a query parameter with multiple values from an IEnumerable
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter key</param>
    /// <param name="values">The parameter values</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Param<T>(string key, IEnumerable<T> values)
    {
        var parameters = (from value in values where value is not null select value.ToString()).ToArray();

        _parameters[key.NotNull()] = new StringValues(parameters);

        return this;
    }

    /// <summary>
    /// Removes a query parameter from the request
    /// </summary>
    /// <param name="key">The parameter key to remove</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest NoParam(string key)
    {
        _parameters.Remove(key.NotNull());

        return this;
    }

    /// <summary>
    /// Attaches HTTP content to the request body
    /// </summary>
    /// <param name="content">The HTTP content to attach</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Attach(HttpContent content)
    {
        Content = content.NotNull();

        return this;
    }

    /// <summary>
    /// Sets the timeout for the request
    /// </summary>
    /// <param name="timeout">The timeout duration</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Timeout(TimeSpan timeout)
    {
        _timeout = timeout;

        return this;
    }

    /// <summary>
    /// Adds a configuration action to be executed before the request is sent
    /// </summary>
    /// <param name="configure">The configuration action to add</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Configure(Action<IHttpRequest> configure)
    {
        _configurations.Add(new Configuration(configure));

        return this;
    }

    /// <summary>
    /// Adds middleware to intercept the request execution
    /// </summary>
    /// <param name="middleware">The middleware function to add</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Intercept(Func<Func<Task<IHttpResponse>>, Task<IHttpResponse>> middleware)
    {
        _middlewares.Add((next, _) => middleware(next));

        return this;
    }

    /// <summary>
    /// Adds middleware to intercept the request execution with access to the request object
    /// </summary>
    /// <param name="middleware">The middleware function to add</param>
    /// <returns>The current request instance for method chaining</returns>
    public IHttpRequest Intercept(Func<Func<Task<IHttpResponse>>, IHttpRequest, Task<IHttpResponse>> middleware)
    {
        _middlewares.Add(new Middleware(middleware));

        return this;
    }

    /// <summary>
    /// Executes the HTTP request asynchronously with the specified completion option
    /// </summary>
    /// <param name="completionOption">The completion option for the HTTP request</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The HTTP response</returns>
    public async Task<IHttpResponse> RunAsync(HttpCompletionOption completionOption, CancellationToken ct = default)
    {
        this.Trace("start");

        var response =
            _middlewares.Count > 0
                ? await InternalRunAsync(0, completionOption, ct).ConfigureAwait(false)
                : await InternalRunAsync(completionOption, ct).ConfigureAwait(false);

        this.Trace("done");

        return response;
    }

    /// <summary>
    /// Executes the HTTP request asynchronously with default completion option
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The HTTP response</returns>
    public Task<IHttpResponse> RunAsync(CancellationToken ct = default)
    {
        return RunAsync(HttpCompletionOption.ResponseContentRead, ct);
    }

    /// <summary>
    /// Internal method to execute the request with middleware at the specified index
    /// </summary>
    /// <param name="middlewareIndex">The current middleware index</param>
    /// <param name="completionOption">The completion option for the HTTP request</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The HTTP response</returns>
    private async Task<IHttpResponse> InternalRunAsync(
        int middlewareIndex,
        HttpCompletionOption completionOption,
        CancellationToken ct
    )
    {
        this.Trace("start {index}/{total}", middlewareIndex + 1, _middlewares.Count);

        if (ct.IsCancellationRequested)
        {
            this.Trace("canceled");
            return HttpResponse.Abort(
                Uri,
                HttpStatusCode.RequestTimeout,
                "Request canceled",
                HttpResponse.EmptyHeaders,
                HttpResponse.EmptyStringContent
            );
        }

        var middleware = _middlewares[middlewareIndex];
        Func<Task<IHttpResponse>> next =
            middlewareIndex + 1 < _middlewares.Count
                ? () => InternalRunAsync(middlewareIndex + 1, completionOption, ct)
                : () => InternalRunAsync(completionOption, ct);

        // call configuration before middleware
        foreach (var configure in _configurations)
            configure(this);

        var response = await middleware(next, this).ConfigureAwait(false);

        this.Trace("done {index}/{total}", middlewareIndex + 1, _middlewares.Count);

        return response;
    }

    /// <summary>
    /// Internal method to execute the actual HTTP request without middleware
    /// </summary>
    /// <param name="completionOption">The completion option for the HTTP request</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The HTTP response</returns>
    private async Task<IHttpResponse> InternalRunAsync(HttpCompletionOption completionOption, CancellationToken ct)
    {
        this.Trace("start");

        if (ct.IsCancellationRequested)
        {
            this.Trace("canceled");
            return HttpResponse.Abort(
                Uri,
                HttpStatusCode.RequestTimeout,
                "Request canceled",
                HttpResponse.EmptyHeaders,
                HttpResponse.EmptyStringContent
            );
        }

        // call configuration exactly before run
        foreach (var configure in _configurations)
            configure(this);

        var uri = Uri;
        var requestMessage = new HttpRequestMessage { Method = Method, RequestUri = uri };

        foreach (var (name, values) in Headers)
            requestMessage.Headers.Add(name, values);

        requestMessage.Content = Content;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                new CancellationTokenSource(_timeout).Token,
                ct
            );

            this.Trace("send request");
            var responseMessage = await _client
                .SendAsync(requestMessage, completionOption, cts.Token)
                .ConfigureAwait(false);

            this.Trace("prepare response");
            var response = HttpResponse.Result(
                responseMessage.IsSuccessStatusCode,
                uri,
                responseMessage.StatusCode,
                responseMessage.ReasonPhrase ?? string.Empty,
                responseMessage.Headers,
                responseMessage.Content
            );

            this.Trace("done");

            return response;
        }
        catch (HttpRequestException e)
        {
            this.Trace("handle connection refused");
            return HttpResponse.NetworkError(
                Uri,
                HttpStatusCode.ServiceUnavailable,
                "Connection refused",
                HttpResponse.EmptyHeaders,
                new StringContent(e.Message)
            );
        }
        catch (OperationCanceledException)
        {
            this.Trace("handle task canceled");
            return HttpResponse.Abort(
                Uri,
                HttpStatusCode.RequestTimeout,
                "Request canceled",
                HttpResponse.EmptyHeaders,
                HttpResponse.EmptyStringContent
            );
        }
    }
}

/// <summary>
/// Helper class providing URI building functionality for HTTP requests
/// </summary>
file static class Helper
{
    /// <summary>
    /// Builds a complete URI from the provided components including query parameters
    /// </summary>
    /// <param name="client">The HTTP client</param>
    /// <param name="baseUri">The base URI</param>
    /// <param name="uri">The relative URI path</param>
    /// <param name="parameters">The query parameters to include</param>
    /// <returns>The complete URI with query parameters</returns>
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

    /// <summary>
    /// Gets a UriFactory instance configured with the provided URI components
    /// </summary>
    /// <param name="client">The HTTP client</param>
    /// <param name="baseUri">The base URI</param>
    /// <param name="uri">The relative URI path</param>
    /// <returns>A configured UriFactory instance</returns>
    /// <exception cref="ArgumentException">Thrown when the request URI is empty and no base address is available</exception>
    public static UriFactory GetUriFactory(HttpClient client, Uri? baseUri, string? uri)
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
