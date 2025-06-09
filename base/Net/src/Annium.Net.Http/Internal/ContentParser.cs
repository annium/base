using System.Net.Http;
using System.Threading.Tasks;

namespace Annium.Net.Http.Internal;

/// <summary>
/// Provides functionality to parse HTTP content into typed objects
/// </summary>
internal static class ContentParser
{
    /// <summary>
    /// Parses HTTP content into a typed object using the specified serializer
    /// </summary>
    /// <typeparam name="T">The type to deserialize the content into</typeparam>
    /// <param name="serializer">The serializer to use for deserialization</param>
    /// <param name="content">The HTTP content to parse</param>
    /// <returns>The deserialized object of type T</returns>
    /// <exception cref="HttpRequestException">Thrown when the media type is missing from the response</exception>
    public static async Task<T> ParseAsync<T>(Serializer serializer, HttpContent content)
    {
        var mediaType =
            content.Headers.ContentType?.MediaType ?? throw new HttpRequestException("Media-type missing in response");

        var raw = await content.ReadAsStringAsync();

        return serializer.Deserialize<T>(mediaType, raw);
    }
}
