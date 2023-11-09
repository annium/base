using System.Buffers.Text;
using System.Text.Json;

namespace Annium.Serialization.Json;

public static class Utf8JsonReaderExtensions
{
    public static decimal GetDecimalFromString(this ref Utf8JsonReader reader)
    {
        var span = reader.ValueSpan;

        if (Utf8Parser.TryParse(span, out decimal value, out var bytesConsumed) && span.Length == bytesConsumed)
        {
            return value.Normalize();
        }

        throw new JsonException("string doesn't contain decimal value");
    }
}
