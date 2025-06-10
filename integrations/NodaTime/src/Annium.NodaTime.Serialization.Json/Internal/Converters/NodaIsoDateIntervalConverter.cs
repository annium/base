using System;
using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using NodaTime.Utility;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Json converter for <see cref="DateInterval"/>.
/// </summary>
internal sealed class NodaIsoDateIntervalConverter : ConverterBase<DateInterval>
{
    /// <summary>
    /// Reads a JSON string representation of a DateInterval in ISO-8601 format ("start/end").
    /// </summary>
    /// <param name="reader">The JSON reader to read from.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The deserialized DateInterval.</returns>
    public override DateInterval ReadImplementation(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new InvalidNodaDataException(
                $"Unexpected token parsing DateInterval. Expected String, got {reader.TokenType}."
            );

        var text = reader.GetString()!;
        var slash = text.IndexOf('/');
        if (slash == -1)
            throw new InvalidNodaDataException("Expected ISO-8601-formatted date interval; slash was missing.");

        var startText = text[..slash];
        if (startText == "")
            throw new InvalidNodaDataException("Expected ISO-8601-formatted date interval; start date was missing.");

        var endText = text[(slash + 1)..];
        if (endText == "")
            throw new InvalidNodaDataException("Expected ISO-8601-formatted date interval; end date was missing.");

        var pattern = LocalDatePattern.Iso;
        var start = pattern.Parse(startText).Value;
        var end = pattern.Parse(endText).Value;

        return new DateInterval(start, end);
    }

    /// <summary>
    /// Writes a DateInterval as an ISO-8601 formatted string ("start/end").
    /// </summary>
    /// <param name="writer">The JSON writer to write to.</param>
    /// <param name="value">The DateInterval value to serialize.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void WriteImplementation(Utf8JsonWriter writer, DateInterval value, JsonSerializerOptions options)
    {
        var pattern = LocalDatePattern.Iso;
        var text = pattern.Format(value.Start) + "/" + pattern.Format(value.End);
        writer.WriteStringValue(text);
    }
}
