using System.Net.Http;
using System.Threading.Tasks;

namespace Annium.Net.Http.Internal;

internal static class ContentParser
{
    public static async Task<T> ParseAsync<T>(
        Serializer serializer,
        HttpContent content
    )
    {
        var mediaType = content.Headers.ContentType?.MediaType
            ?? throw new HttpRequestException("Media-type missing in response");

        var raw = await content.ReadAsStringAsync();

        return serializer.Deserialize<T>(mediaType, raw);
    }
}