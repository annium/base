using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Http.Internal;
using OneOf;

namespace Annium.Net.Http.Extensions;

/// <summary>
/// Extension methods for converting HTTP request responses to typed objects
/// </summary>
public static class AsExtensions
{
    /// <summary>
    /// Executes the HTTP request and attempts to parse the response to the specified type
    /// </summary>
    /// <typeparam name="T">The type to parse the response to</typeparam>
    /// <param name="request">The HTTP request to execute</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The parsed response or default value if parsing fails</returns>
    public static async Task<T?> AsAsync<T>(this IHttpRequest request, CancellationToken ct = default)
    {
        var response = await request.RunAsync(ct);
        if (response.IsAbort)
            return default;

        try
        {
            var data = await ContentParser.ParseAsync<T>(request.Serializer, response.Content);

            return data;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Executes the HTTP request and attempts to parse the response to the specified type with a fallback default value
    /// </summary>
    /// <typeparam name="T">The type to parse the response to</typeparam>
    /// <param name="request">The HTTP request to execute</param>
    /// <param name="defaultData">The default value to return if parsing fails</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The parsed response or the provided default value if parsing fails</returns>
    public static async Task<T> AsAsync<T>(this IHttpRequest request, T defaultData, CancellationToken ct = default)
    {
        var response = await request.RunAsync(ct);
        if (response.IsAbort)
            return defaultData;

        try
        {
            var data = await ContentParser.ParseAsync<T>(request.Serializer, response.Content);

            return data ?? defaultData;
        }
        catch
        {
            return defaultData;
        }
    }

    /// <summary>
    /// Executes the HTTP request and attempts to parse the response as either success or failure type
    /// </summary>
    /// <typeparam name="TSuccess">The type for successful response</typeparam>
    /// <typeparam name="TFailure">The type for failure response</typeparam>
    /// <param name="request">The HTTP request to execute</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A union type containing either the success or failure result</returns>
    public static async Task<OneOf<TSuccess, TFailure?>> AsAsync<TSuccess, TFailure>(
        this IHttpRequest request,
        CancellationToken ct = default
    )
    {
        var response = await request.RunAsync(ct);
        if (response.IsAbort)
            return default(TFailure);

        try
        {
            var success = await ContentParser.ParseAsync<TSuccess>(request.Serializer, response.Content);
            if (!Equals(success, default(TSuccess)))
                return success;

            var failure = await ContentParser.ParseAsync<TFailure>(request.Serializer, response.Content);
            if (!Equals(failure, default(TFailure)))
                return failure;

            return default(TFailure);
        }
        catch
        {
            return default(TFailure);
        }
    }

    /// <summary>
    /// Executes the HTTP request and attempts to parse the response as either success or failure type with custom failure handling
    /// </summary>
    /// <typeparam name="TSuccess">The type for successful response</typeparam>
    /// <typeparam name="TFailure">The type for failure response</typeparam>
    /// <param name="request">The HTTP request to execute</param>
    /// <param name="getFailure">Function to handle failure scenarios</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A union type containing either the success or failure result</returns>
    public static async Task<OneOf<TSuccess, TFailure>> AsAsync<TSuccess, TFailure>(
        this IHttpRequest request,
        Func<HttpFailureReason, IHttpResponse, Exception?, Task<TFailure>> getFailure,
        CancellationToken ct = default
    )
    {
        var response = await request.RunAsync(ct);
        if (response.IsAbort)
            return await getFailure(HttpFailureReason.Abort, response, null);

        try
        {
            var success = await ContentParser.ParseAsync<TSuccess>(request.Serializer, response.Content);
            if (!Equals(success, default(TSuccess)))
                return success;

            var failure = await ContentParser.ParseAsync<TFailure>(request.Serializer, response.Content);
            if (!Equals(failure, default(TFailure)))
                return failure;

            return await getFailure(HttpFailureReason.Parse, response, null);
        }
        catch (Exception e)
        {
            return await getFailure(HttpFailureReason.Exception, response, e);
        }
    }
}
