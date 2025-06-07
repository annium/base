using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http;

/// <summary>
/// Represents an HTTP response with typed data
/// </summary>
/// <typeparam name="T">The type of the response data</typeparam>
public interface IHttpResponse<T> : IHttpResponse
{
    /// <summary>
    /// Gets the typed response data
    /// </summary>
    T Data { get; }
}

/// <summary>
/// Represents an HTTP response
/// </summary>
public interface IHttpResponse
{
    /// <summary>
    /// Gets a value indicating whether the request was aborted
    /// </summary>
    bool IsAbort { get; }

    /// <summary>
    /// Gets a value indicating whether the response indicates success
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the response indicates failure
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets the HTTP status code of the response
    /// </summary>
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the status text of the response
    /// </summary>
    string StatusText { get; }

    /// <summary>
    /// Gets the URI that generated this response
    /// </summary>
    Uri Uri { get; }

    /// <summary>
    /// Gets the HTTP response headers
    /// </summary>
    HttpResponseHeaders Headers { get; }

    /// <summary>
    /// Gets the HTTP response content
    /// </summary>
    HttpContent Content { get; }
}
