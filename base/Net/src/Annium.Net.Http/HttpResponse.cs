using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http;

/// <summary>
/// Represents an HTTP response with typed data
/// </summary>
/// <typeparam name="T">The type of the response data</typeparam>
public class HttpResponse<T> : HttpResponse, IHttpResponse<T>
{
    /// <summary>
    /// Gets the typed response data
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// Initializes a new instance of the HttpResponse class
    /// </summary>
    /// <param name="message">The base HTTP response</param>
    /// <param name="data">The typed response data</param>
    public HttpResponse(IHttpResponse message, T data)
        : base(message)
    {
        Data = data;
    }
}

/// <summary>
/// Represents an HTTP response
/// </summary>
public class HttpResponse : IHttpResponse
{
    /// <summary>
    /// Default HTTP response headers used for aborted requests
    /// </summary>
    private static readonly HttpResponseHeaders _defaultHeaders;

    static HttpResponse()
    {
        using var message = new HttpResponseMessage();
        _defaultHeaders = message.Headers;
    }

    /// <summary>
    /// Gets a value indicating whether the request was aborted
    /// </summary>
    public bool IsAbort { get; }

    /// <summary>
    /// Gets a value indicating whether the response indicates success
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the response indicates failure
    /// </summary>
    public bool IsFailure { get; }

    /// <summary>
    /// Gets the HTTP status code of the response
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the status text of the response
    /// </summary>
    public string StatusText { get; }

    /// <summary>
    /// Gets the URI that generated this response
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    /// Gets the HTTP response headers
    /// </summary>
    public HttpResponseHeaders Headers { get; }

    /// <summary>
    /// Gets the HTTP response content
    /// </summary>
    public HttpContent Content { get; }

    /// <summary>
    /// Initializes a new instance of the HttpResponse class from an HttpResponseMessage
    /// </summary>
    /// <param name="uri">The URI that generated this response</param>
    /// <param name="message">The HttpResponseMessage</param>
    public HttpResponse(Uri uri, HttpResponseMessage message)
    {
        IsAbort = false;
        IsSuccess = message.IsSuccessStatusCode;
        IsFailure = !message.IsSuccessStatusCode;
        StatusCode = message.StatusCode;
        StatusText = message.ReasonPhrase ?? string.Empty;
        Uri = uri;
        Headers = message.Headers;
        Content = message.Content;
    }

    /// <summary>
    /// Initializes a new instance of the HttpResponse class for an aborted request
    /// </summary>
    /// <param name="uri">The URI that generated this response</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="statusText">The status text</param>
    /// <param name="message">The response message</param>
    internal HttpResponse(Uri uri, HttpStatusCode statusCode, string statusText, string message)
    {
        IsAbort = true;
        IsSuccess = false;
        IsFailure = false;
        StatusCode = statusCode;
        StatusText = statusText;
        Uri = uri;
        Headers = _defaultHeaders;
        Content = new StringContent(message);
    }

    /// <summary>
    /// Initializes a new instance of the HttpResponse class from another response
    /// </summary>
    /// <param name="response">The source response</param>
    protected HttpResponse(IHttpResponse response)
    {
        IsAbort = response.IsAbort;
        IsSuccess = response.IsSuccess;
        IsFailure = response.IsFailure;
        StatusCode = response.StatusCode;
        StatusText = response.StatusText;
        Uri = response.Uri;
        Headers = response.Headers;
        Content = response.Content;
    }
}
