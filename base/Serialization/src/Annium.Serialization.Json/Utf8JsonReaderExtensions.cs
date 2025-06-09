using System.Buffers.Text;
using System.Text.Json;

namespace Annium.Serialization.Json;

/// <summary>
/// Extension methods for Utf8JsonReader providing additional parsing functionality.
/// </summary>
public static class Utf8JsonReaderExtensions
{
    /// <summary>
    /// Reads a decimal value from a string token in the JSON reader.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <returns>The parsed decimal value.</returns>
    /// <exception cref="JsonException">Thrown when the string doesn't contain a valid decimal value.</exception>
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
