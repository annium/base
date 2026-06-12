using System;
using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using NodaTime.Utility;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Json converter for <see cref="Interval"/>.
/// </summary>
internal sealed class NodaIsoIntervalConverter : ConverterBase<Interval>
{
    /// <summary>
    /// The extended ISO-8601 Instant pattern used to parse and format the interval endpoints.
    /// </summary>
    private static readonly InstantPattern _pattern = InstantPattern.ExtendedIso;

    /// <summary>
    /// Reads a JSON string representation of an Interval in ISO-8601 format ("start/end", allowing empty start or end).
    /// </summary>
    /// <param name="reader">The JSON reader to read from.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The deserialized Interval.</returns>
    public override Interval ReadImplementation(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new InvalidNodaDataException(
                $"Unexpected token parsing Interval. Expected String, got {reader.TokenType}."
            );

        var text = reader.GetString()!; // non-null: String token verified above
        var slash = text.IndexOf('/');
        if (slash == -1)
            throw new InvalidNodaDataException("Expected ISO-8601-formatted interval; slash was missing.");

        var startText = text[..slash];
        var endText = text[(slash + 1)..];
        var start = startText == "" ? (Instant?)null : _pattern.Parse(startText).Value;
        var end = endText == "" ? (Instant?)null : _pattern.Parse(endText).Value;

        return new Interval(start, end);
    }

    /// <summary>
    /// Writes an Interval as an ISO-8601 formatted string ("start/end", with empty strings for null start or end).
    /// </summary>
    /// <param name="writer">The JSON writer to write to.</param>
    /// <param name="value">The Interval value to serialize.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void WriteImplementation(Utf8JsonWriter writer, Interval value, JsonSerializerOptions options)
    {
        var text =
            (value.HasStart ? _pattern.Format(value.Start) : "")
            + "/"
            + (value.HasEnd ? _pattern.Format(value.End) : "");
        writer.WriteStringValue(text);
    }
}
