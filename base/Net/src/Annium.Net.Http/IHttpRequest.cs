using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Http;

/// <summary>
/// Represents an HTTP request with fluent configuration capabilities
/// </summary>
public interface IHttpRequest : ILogSubject
{
    /// <summary>
    /// Gets the HTTP method for the request
    /// </summary>
    HttpMethod Method { get; }

    /// <summary>
    /// Gets the complete URI for the request
    /// </summary>
    Uri Uri { get; }

    /// <summary>
    /// Gets the path component of the URI
    /// </summary>
    Uri Path { get; }

    /// <summary>
    /// Gets the HTTP request headers
    /// </summary>
    HttpRequestHeaders Headers { get; }

    /// <summary>
    /// Gets the query parameters for the request
    /// </summary>
    IReadOnlyDictionary<string, StringValues> Params { get; }

    /// <summary>
    /// Gets the HTTP content for the request
    /// </summary>
    HttpContent? Content { get; }

    /// <summary>
    /// Sets the base URI for the request
    /// </summary>
    /// <param name="baseUri">The base URI</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Base(Uri baseUri);

    /// <summary>
    /// Sets the base URI for the request
    /// </summary>
    /// <param name="baseUri">The base URI string</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Base(string baseUri);

    /// <summary>
    /// Sets the HTTP method and URI for the request
    /// </summary>
    /// <param name="method">The HTTP method</param>
    /// <param name="uri">The URI</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest With(HttpMethod method, Uri uri);

    /// <summary>
    /// Sets the HTTP method and URI for the request
    /// </summary>
    /// <param name="method">The HTTP method</param>
    /// <param name="uri">The URI string</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest With(HttpMethod method, string uri);

    /// <summary>
    /// Adds a header to the request
    /// </summary>
    /// <param name="name">The header name</param>
    /// <param name="value">The header value</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Header(string name, string value);

    /// <summary>
    /// Adds a header with multiple values to the request
    /// </summary>
    /// <param name="name">The header name</param>
    /// <param name="values">The header values</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Header(string name, IEnumerable<string> values);

    /// <summary>
    /// Sets the Authorization header for the request
    /// </summary>
    /// <param name="value">The authentication header value</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Authorization(AuthenticationHeaderValue value);

    /// <summary>
    /// Adds a query parameter to the request
    /// </summary>
    /// <typeparam name="T">The type of the parameter value</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="value">The parameter value</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Param<T>(string key, T value);

    /// <summary>
    /// Adds a query parameter with multiple values from a List
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Param<T>(string key, List<T> values);

    /// <summary>
    /// Adds a query parameter with multiple values from an IList
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Param<T>(string key, IList<T> values);

    /// <summary>
    /// Adds a query parameter with multiple values from an IReadOnlyList
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Param<T>(string key, IReadOnlyList<T> values);

    /// <summary>
    /// Adds a query parameter with multiple values from an IReadOnlyCollection
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Param<T>(string key, IReadOnlyCollection<T> values);

    /// <summary>
    /// Adds a query parameter with multiple values from an array
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Param<T>(string key, T[] values);

    /// <summary>
    /// Adds a query parameter with multiple values from an IEnumerable
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Param<T>(string key, IEnumerable<T> values);

    /// <summary>
    /// Removes a query parameter from the request
    /// </summary>
    /// <param name="key">The parameter name to remove</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest NoParam(string key);

    /// <summary>
    /// Attaches HTTP content to the request
    /// </summary>
    /// <param name="content">The content to attach</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Attach(HttpContent content);

    /// <summary>
    /// Sets the timeout for the request
    /// </summary>
    /// <param name="timeout">The timeout duration</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Timeout(TimeSpan timeout);

    /// <summary>
    /// Configures the request using a delegate
    /// </summary>
    /// <param name="configure">The configuration delegate</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Configure(Action<IHttpRequest> configure);

    /// <summary>
    /// Adds middleware to intercept the request execution
    /// </summary>
    /// <param name="middleware">The middleware delegate</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Intercept(Func<Func<Task<IHttpResponse>>, Task<IHttpResponse>> middleware);

    /// <summary>
    /// Adds middleware to intercept the request execution with access to the request
    /// </summary>
    /// <param name="middleware">The middleware delegate</param>
    /// <returns>This request instance for method chaining</returns>
    IHttpRequest Intercept(Func<Func<Task<IHttpResponse>>, IHttpRequest, Task<IHttpResponse>> middleware);

    /// <summary>
    /// Executes the HTTP request asynchronously with the specified completion option
    /// </summary>
    /// <param name="completionOption">The completion option</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The HTTP response</returns>
    Task<IHttpResponse> RunAsync(HttpCompletionOption completionOption, CancellationToken ct = default);

    /// <summary>
    /// Executes the HTTP request asynchronously
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The HTTP response</returns>
    Task<IHttpResponse> RunAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the serializer for content processing
    /// </summary>
    internal Serializer Serializer { get; }
}
