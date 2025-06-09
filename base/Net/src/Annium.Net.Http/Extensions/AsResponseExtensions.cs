using System;
using System.Threading;
using System.Threading.Tasks;
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
            var data = await ContentParser.ParseAsync<T>(request.Serializer, response.Content);

            return new HttpResponse<T?>(response, data);
        }
        catch
        {
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
            var data = await ContentParser.ParseAsync<T>(request.Serializer, response.Content);
            return new HttpResponse<T>(response, data ?? defaultData);
        }
        catch
        {
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
        if (response.IsAbort)
            return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, default(TFailure));

        try
        {
            var success = await ContentParser.ParseAsync<TSuccess>(request.Serializer, response.Content);
            if (!Equals(success, default(TSuccess)))
                return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, success);

            var failure = await ContentParser.ParseAsync<TFailure>(request.Serializer, response.Content);
            if (!Equals(failure, default(TFailure)))
                return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, failure);

            return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, default(TFailure));
        }
        catch
        {
            return new HttpResponse<OneOf<TSuccess, TFailure?>>(response, default(TFailure));
        }
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
        if (response.IsAbort)
            return new HttpResponse<OneOf<TSuccess, TFailure>>(
                response,
                await getFailure(HttpFailureReason.Abort, response, null)
            );

        try
        {
            var success = await ContentParser.ParseAsync<TSuccess>(request.Serializer, response.Content);
            if (!Equals(success, default(TSuccess)))
                return new HttpResponse<OneOf<TSuccess, TFailure>>(response, success);

            var failure = await ContentParser.ParseAsync<TFailure>(request.Serializer, response.Content);
            if (!Equals(failure, default(TFailure)))
                return new HttpResponse<OneOf<TSuccess, TFailure>>(response, failure);

            return new HttpResponse<OneOf<TSuccess, TFailure>>(
                response,
                await getFailure(HttpFailureReason.Parse, response, null)
            );
        }
        catch (Exception e)
        {
            return new HttpResponse<OneOf<TSuccess, TFailure>>(
                response,
                await getFailure(HttpFailureReason.Exception, response, e)
            );
        }
    }
}
