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
    public override DateInterval ReadImplementation(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new InvalidNodaDataException($"Unexpected token parsing DateInterval. Expected String, got {reader.TokenType}.");

        var text = reader.GetString()!;
        var slash = text.IndexOf('/');
        if (slash == -1)
            throw new InvalidNodaDataException("Expected ISO-8601-formatted date interval; slash was missing.");

        var startText = text.Substring(0, slash);
        if (startText == "")
            throw new InvalidNodaDataException("Expected ISO-8601-formatted date interval; start date was missing.");

        var endText = text.Substring(slash + 1);
        if (endText == "")
            throw new InvalidNodaDataException("Expected ISO-8601-formatted date interval; end date was missing.");

        var pattern = LocalDatePattern.Iso;
        var start = pattern.Parse(startText).Value;
        var end = pattern.Parse(endText).Value;

        return new DateInterval(start, end);
    }

    public override void WriteImplementation(
        Utf8JsonWriter writer,
        DateInterval value,
        JsonSerializerOptions options
    )
    {
        var pattern = LocalDatePattern.Iso;
        var text = pattern.Format(value.Start) + "/" + pattern.Format(value.End);
        writer.WriteStringValue(text);
    }
}