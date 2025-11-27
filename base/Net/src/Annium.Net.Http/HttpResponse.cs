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
    /// Default HTTP response headers, useful for network error / aborted requests
    /// </summary>
    public static readonly HttpResponseHeaders EmptyHeaders;

    /// <summary>
    /// Empty StringContent, useful for network error / aborted requests
    /// </summary>
    public static HttpContent EmptyStringContent => new StringContent(string.Empty);

    static HttpResponse()
    {
        using var message = new HttpResponseMessage();
        EmptyHeaders = message.Headers;
    }

    /// <summary>
    /// Initializes a new instance of the HttpResponse class for a network failed request
    /// </summary>
    /// <param name="uri">The URI that generated this response</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="statusText">HTTP status text</param>
    /// <param name="headers">HttpResponseHeaders for manually created NetworkError HttpResponse</param>
    /// <param name="content">HttpContent for manually created NetworkError HttpResponse</param>
    /// <returns>new HttpResponse with specified parameters</returns>
    public static HttpResponse NetworkError(
        Uri uri,
        HttpStatusCode statusCode,
        string statusText,
        HttpResponseHeaders headers,
        HttpContent content
    )
    {
        return new HttpResponse(true, false, false, false, statusCode, statusText, uri, headers, content);
    }

    /// <summary>
    /// Initializes a new instance of the HttpResponse class for a client aborted request
    /// </summary>
    /// <param name="uri">The URI that generated this response</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="statusText">HTTP status text</param>
    /// <param name="headers">HttpResponseHeaders for manually created Abort HttpResponse</param>
    /// <param name="content">HttpContent for manually created Abort HttpResponse</param>
    /// <returns>new HttpResponse with specified parameters</returns>
    public static HttpResponse Abort(
        Uri uri,
        HttpStatusCode statusCode,
        string statusText,
        HttpResponseHeaders headers,
        HttpContent content
    )
    {
        return new HttpResponse(false, true, false, false, statusCode, statusText, uri, headers, content);
    }

    /// <summary>
    /// Initializes a new instance of the HttpResponse class for a successful response
    /// </summary>
    /// <param name="isSuccess">Flag indicating whether response is successful</param>
    /// <param name="uri">The URI that generated this response</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="statusText">HTTP status text</param>
    /// <param name="headers">HttpResponseHeaders from resulting HttpResponseMessage</param>
    /// <param name="content">HttpContent from resulting HttpResponseMessage</param>
    /// <returns>new HttpResponse with specified parameters</returns>
    public static HttpResponse Result(
        bool isSuccess,
        Uri uri,
        HttpStatusCode statusCode,
        string statusText,
        HttpResponseHeaders headers,
        HttpContent content
    )
    {
        return new HttpResponse(false, false, isSuccess, !isSuccess, statusCode, statusText, uri, headers, content);
    }

    /// <summary>
    /// Gets a value indicating whether the request was failed to be sent due to network error
    /// </summary>
    public bool IsNetworkError { get; }

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
    /// Initializes a new instance of the HttpResponse class from another response
    /// </summary>
    /// <param name="isNetworkError">Flag, indicating simulated response for network failure</param>
    /// <param name="isAbort">Flag, indicating simulated response for request, aborted by client</param>
    /// <param name="isSuccess">Flag, indicating successful response</param>
    /// <param name="isFailure">Flag, indicating failed response</param>
    /// <param name="statusCode">Response status code</param>
    /// <param name="statusText">Response status text</param>
    /// <param name="uri">Request original Uri</param>
    /// <param name="headers">Response headers</param>
    /// <param name="content">Response content</param>
    private HttpResponse(
        bool isNetworkError,
        bool isAbort,
        bool isSuccess,
        bool isFailure,
        HttpStatusCode statusCode,
        string statusText,
        Uri uri,
        HttpResponseHeaders headers,
        HttpContent content
    )
    {
        IsNetworkError = isNetworkError;
        IsAbort = isAbort;
        IsSuccess = isSuccess;
        IsFailure = isFailure;
        StatusCode = statusCode;
        StatusText = statusText;
        Uri = uri;
        Headers = headers;
        Content = content;
    }

    /// <summary>
    /// Initializes a new instance of the HttpResponse class from another response
    /// </summary>
    /// <param name="response">The source response</param>
    protected HttpResponse(IHttpResponse response)
        : this(
            response.IsNetworkError,
            response.IsAbort,
            response.IsSuccess,
            response.IsFailure,
            response.StatusCode,
            response.StatusText,
            response.Uri,
            response.Headers,
            response.Content
        )
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
