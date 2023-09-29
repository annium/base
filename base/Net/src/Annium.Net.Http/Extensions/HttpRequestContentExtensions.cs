using System.Net.Http;
using System.Net.Mime;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class HttpRequestContentExtensions
{
    public static IHttpRequest JsonContent<T>(this IHttpRequest request, T data)
    {
        var content = request.Serializer.Serialize(MediaTypeNames.Application.Json, data);

        return request.Attach(new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));
    }

    public static IHttpRequest StringContent(this IHttpRequest request, string data, string mimeType = "text/plain")
    {
        return request.Attach(new StringContent(data, Encoding.UTF8, mimeType));
    }
}