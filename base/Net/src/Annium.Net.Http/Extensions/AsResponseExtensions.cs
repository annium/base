using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Http.Internal;
using OneOf;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

/// <summary>
/// Extension methods for converting HTTP request responses to typed HTTP response objects
/// </summary>
public static class AsResponseExtensions
{
    /// <summary>
    /// Executes the HTTP request and returns a response with parsed content
    /// </summary>
    /// <typeparam name="T">The type to parse the response content to</typeparam>
    /// <param name="request">The HTTP request to execute</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>An HTTP response containing the parsed content or default value</returns>
    public static async Task<IHttpResponse<T?>> AsResponseAsync<T>(
        this IHttpRequest request,
        CancellationToken ct = default
    )
    {
        var response = await request.RunAsync(ct);
        if (response.IsAbort)
            return new HttpResponse<T?>(response, default);

        try
        {
            var data = await ContentParser.ParseAsync<T>(request.GetSerializer(), response.Content);

            return new HttpResponse<T?>(response, data);
        }
        catch (Exception e)
        {
            request.Error(e);
            return new HttpResponse<T?>(response, default);
        }
    }

    /// <summary>
    /// Executes the HTTP request and returns a response with parsed content or default value
    /// </summary>
    /// <typeparam name="T">The type to parse the response content to</typeparam>
    /// <param name="request">The HTTP request to execute</param>
    /// <param name="defaultData">The default value to use if parsing fails</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>An HTTP response containing the parsed content or the default value</returns>
    public static async Task<IHttpResponse<T>> AsResponseAsync<T>(
        this IHttpRequest request,
        T defaultData,
        CancellationToken ct = default
    )
    {
        var response = await request.RunAsync(ct);
        if (response.IsAbort)
            return new HttpResponse<T>(response, defaultData);

        try
        {
            var data = await ContentParser.ParseAsync<T>(request.GetSerializer(), response.Content);
            return new HttpResponse<T>(response, data ?? defaultData);
        }
        catch (Exception e)
        {
            request.Error(e);
            return new HttpResponse<T>(response, defaultData);
        }
    }

    /// <summary>
    /// Executes the HTTP request and returns a response with content parsed as either success or failure type
    /// </summary>
    /// <typeparam name="TSuccess">The type for successful response</typeparam>
    /// <typeparam name="TFailure">The type for failure response</typeparam>
    /// <param name="request">The HTTP request to execute</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>An HTTP response containing a union type with either success or failure result</returns>
    public static async Task<IHttpResponse<OneOf<TSuccess, TFailure?>>> AsResponseAsync<TSuccess, TFailure>(
        this IHttpRequest request,
        CancellationToken ct = default
    )
    {
        var response = await request.RunAsync(ct);

        if (response.IsNetworkError)
            return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, default(TFailure));

        if (response.IsAbort)
            return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, default(TFailure));

        var (success, successParsed, _) = await TryParseAsync<TSuccess>(request, response);
        if (successParsed)
            return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, success);

        var (failure, failureParsed, _) = await TryParseAsync<TFailure>(request, response);
        if (failureParsed)
            return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, failure);

        return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, default(TFailure));
    }

    /// <summary>
    /// Executes the HTTP request and returns a response with content parsed as either success or failure type with custom failure handling
    /// </summary>
    /// <typeparam name="TSuccess">The type for successful response</typeparam>
    /// <typeparam name="TFailure">The type for failure response</typeparam>
    /// <param name="request">The HTTP request to execute</param>
    /// <param name="getFailure">Function to handle failure scenarios</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>An HTTP response containing a union type with either success or failure result</returns>
    public static async Task<IHttpResponse<OneOf<TSuccess, TFailure>>> AsResponseAsync<TSuccess, TFailure>(
        this IHttpRequest request,
        Func<HttpFailureReason, IHttpResponse, Exception?, Task<TFailure>> getFailure,
        CancellationToken ct = default
    )
    {
        var response = await request.RunAsync(ct);

        if (response.IsNetworkError)
            return new HttpResponse<OneOf<TSuccess, TFailure>>(
                response,
                await getFailure(HttpFailureReason.Network, response, null)
            );

        if (response.IsAbort)
            return new HttpResponse<OneOf<TSuccess, TFailure>>(
                response,
                await getFailure(HttpFailureReason.Abort, response, null)
            );

        var (success, successParsed, successError) = await TryParseAsync<TSuccess>(request, response);
        if (successParsed)
            return new HttpResponse<OneOf<TSuccess, TFailure>>(response, success);

        var (failure, failureParsed, failureError) = await TryParseAsync<TFailure>(request, response);
        if (failureParsed)
            return new HttpResponse<OneOf<TSuccess, TFailure>>(response, failure);

        // neither shape read. An exception from either attempt is the more specific answer, and the
        // success one is reported in preference because it is the shape the caller asked for
        var error = successError ?? failureError;

        return new HttpResponse<OneOf<TSuccess, TFailure>>(
            response,
            error is null
                ? await getFailure(HttpFailureReason.Parse, response, null)
                : await getFailure(HttpFailureReason.Exception, response, error)
        );
    }

    /// <summary>
    /// Attempts to read the response body as <typeparamref name="T"/>, reporting whether it produced
    /// anything and what stopped it if not.
    /// </summary>
    /// <remarks>
    /// Each shape of a union is tried independently, and that is the point of this method existing. Parsing
    /// them in one <c>try</c> meant a throw on the first abandoned the second: a body that is an error
    /// object, against a success type that is a collection, threw and the error shape was never read - so
    /// the caller was told the response could not be parsed while the server had said plainly what was
    /// wrong. The two shapes are alternatives; failing to be one is not a reason to stop asking about the
    /// other.
    /// </remarks>
    /// <typeparam name="T">The type to read the body as.</typeparam>
    /// <param name="request">The request, used for its serializer and to log a failed attempt.</param>
    /// <param name="response">The response whose body is read.</param>
    /// <returns>The value, whether it parsed into something other than its default, and any exception raised.</returns>
    private static async Task<(T Value, bool Parsed, Exception? Error)> TryParseAsync<T>(
        IHttpRequest request,
        IHttpResponse response
    )
    {
        try
        {
            var value = await ContentParser.ParseAsync<T>(request.GetSerializer(), response.Content);

            return (value, !Equals(value, default(T)), null);
        }
        catch (Exception e)
        {
            request.Error(e);

            return (default!, false, e);
        }
    }
}
