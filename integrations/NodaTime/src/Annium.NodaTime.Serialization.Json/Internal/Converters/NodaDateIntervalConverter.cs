using System;
using System.Text.Json;
using NodaTime;
using NodaTime.Utility;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Json converter for <see cref="DateInterval"/> using a compound representation. The start and
/// end aspects of the date interval are represented with separate properties, each parsed and formatted
/// by the <see cref="LocalDate"/> converter for the serializer provided.
/// </summary>
internal sealed class NodaDateIntervalConverter : ConverterBase<DateInterval>
{
    public override DateInterval ReadImplementation(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        LocalDate? startLocalDate = null;
        LocalDate? endLocalDate = null;

        var depth = reader.CurrentDepth;
        while (reader.Read() && reader.CurrentDepth > depth)
        {
            if (reader.HasProperty(nameof(Interval.Start)))
                startLocalDate = JsonSerializer.Deserialize<LocalDate>(ref reader, options);

            else if (reader.HasProperty(nameof(Interval.End)))
                endLocalDate = JsonSerializer.Deserialize<LocalDate>(ref reader, options);
        }

        if (!startLocalDate.HasValue)
            throw new InvalidNodaDataException("Expected date interval; start date was missing.");

        if (!endLocalDate.HasValue)
            throw new InvalidNodaDataException("Expected date interval; end date was missing.");

        return new DateInterval(startLocalDate.Value, endLocalDate.Value);
    }

    public override void WriteImplementation(
        Utf8JsonWriter writer,
        DateInterval value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(Interval.Start).CamelCase());
        JsonSerializer.Serialize(writer, value.Start, options);

        writer.WritePropertyName(nameof(Interval.End).CamelCase());
        JsonSerializer.Serialize(writer, value.End, options);

        writer.WriteEndObject();
    }
}