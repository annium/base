using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Http.Internal;
using OneOf;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class AsResponseExtensions
{
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

    public static async Task<IHttpResponse<OneOf<TSuccess?, TFailure>>> AsResponseAsync<TSuccess, TFailure>(
        this IHttpRequest request,
        CancellationToken ct = default
    )
    {
        var response = await request.RunAsync(ct);
        if (response.IsAbort)
            return new HttpResponse<OneOf<TSuccess?, TFailure>>(response, default(TSuccess));

        try
        {
            var success = await ContentParser.ParseAsync<TSuccess>(request.Serializer, response.Content);
            if (!Equals(success, default(TSuccess)))
                return new HttpResponse<OneOf<TSuccess?, TFailure>>(response, success);

            var failure = await ContentParser.ParseAsync<TFailure>(request.Serializer, response.Content);
            if (!Equals(failure, default(TFailure)))
                return new HttpResponse<OneOf<TSuccess?, TFailure>>(response, failure);

            return new HttpResponse<OneOf<TSuccess?, TFailure>>(response, default(TSuccess));
        }
        catch
        {
            return new HttpResponse<OneOf<TSuccess?, TFailure>>(response, default(TSuccess));
        }
    }

    public static async Task<IHttpResponse<OneOf<TSuccess, TFailure>>> AsResponseAsync<TSuccess, TFailure>(
        this IHttpRequest request,
        TSuccess defaultData,
        CancellationToken ct = default
    )
    {
        var response = await request.RunAsync(ct);
        if (response.IsAbort)
            return new HttpResponse<OneOf<TSuccess, TFailure>>(response, defaultData);

        try
        {
            var success = await ContentParser.ParseAsync<TSuccess>(request.Serializer, response.Content);
            if (!Equals(success, default(TSuccess)))
                return new HttpResponse<OneOf<TSuccess, TFailure>>(response, success);

            var failure = await ContentParser.ParseAsync<TFailure>(request.Serializer, response.Content);
            if (!Equals(failure, default(TFailure)))
                return new HttpResponse<OneOf<TSuccess, TFailure>>(response, failure);

            return new HttpResponse<OneOf<TSuccess, TFailure>>(response, defaultData);
        }
        catch
        {
            return new HttpResponse<OneOf<TSuccess, TFailure>>(response, defaultData);
        }
    }
}