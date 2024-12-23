using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Http.Internal;
using OneOf;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class AsExtensions
{
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
