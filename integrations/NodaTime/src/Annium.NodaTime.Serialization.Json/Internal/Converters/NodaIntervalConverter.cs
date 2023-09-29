using System;
using System.Text.Json;
using NodaTime;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Json converter for <see cref="Interval"/> using a compound representation. The start and
/// end aspects of the interval are represented with separate properties, each parsed and formatted
/// by the <see cref="Instant"/> converter for the serializer provided.
/// </summary>
internal sealed class NodaIntervalConverter : ConverterBase<Interval>
{
    public override Interval ReadImplementation(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        Instant? startInstant = null;
        Instant? endInstant = null;

        var depth = reader.CurrentDepth;
        while (reader.Read() && reader.CurrentDepth > depth)
        {
            if (reader.HasProperty(nameof(Interval.Start)))
                startInstant = JsonSerializer.Deserialize<Instant>(ref reader, options);
            else if (reader.HasProperty(nameof(Interval.End)))
                endInstant = JsonSerializer.Deserialize<Instant>(ref reader, options);
        }

        return new Interval(startInstant, endInstant);
    }

    public override void WriteImplementation(
        Utf8JsonWriter writer,
        Interval value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();
        if (value.HasStart)
        {
            writer.WritePropertyName(nameof(Interval.Start).CamelCase());
            JsonSerializer.Serialize(writer, value.Start, options);
        }

        if (value.HasEnd)
        {
            writer.WritePropertyName(nameof(Interval.End).CamelCase());
            JsonSerializer.Serialize(writer, value.End, options);
        }

        writer.WriteEndObject();
    }
}