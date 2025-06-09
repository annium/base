using System.Net.Http;
using System.Net.Mime;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

/// <summary>
/// Extension methods for attaching content to HTTP requests
/// </summary>
public static class HttpRequestContentExtensions
{
    /// <summary>
    /// Attaches JSON content to the HTTP request
    /// </summary>
    /// <typeparam name="T">The type of data to serialize</typeparam>
    /// <param name="request">The HTTP request</param>
    /// <param name="data">The data to serialize as JSON</param>
    /// <returns>The HTTP request with attached JSON content</returns>
    public static IHttpRequest JsonContent<T>(this IHttpRequest request, T data)
    {
        var content = request.Serializer.Serialize(MediaTypeNames.Application.Json, data);

        return request.Attach(new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));
    }

    /// <summary>
    /// Attaches string content to the HTTP request
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="data">The string data to attach</param>
    /// <param name="mimeType">The MIME type of the content</param>
    /// <returns>The HTTP request with attached string content</returns>
    public static IHttpRequest StringContent(this IHttpRequest request, string data, string mimeType = "text/plain")
    {
        return request.Attach(new StringContent(data, Encoding.UTF8, mimeType));
    }
}
