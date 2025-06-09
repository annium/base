using System;
using System.Text.Json;
using NodaTime;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Json converter for <see cref="DateTimeZone"/>.
/// </summary>
internal sealed class NodaDateTimeZoneConverter : ConverterBase<DateTimeZone>
{
    /// <summary>
    /// The date time zone provider used to resolve time zone IDs to DateTimeZone instances.
    /// </summary>
    private readonly IDateTimeZoneProvider _provider;

    /// <summary>
    /// Initializes a new instance of the NodaDateTimeZoneConverter class.
    /// </summary>
    /// <param name="provider">Provides the <see cref="DateTimeZone"/> that corresponds to each time zone ID in the JSON string.</param>
    public NodaDateTimeZoneConverter(IDateTimeZoneProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Reads a JSON string representation of a time zone ID and converts it to a DateTimeZone.
    /// </summary>
    /// <param name="reader">The JSON reader to read from.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The DateTimeZone corresponding to the time zone ID.</returns>
    public override DateTimeZone ReadImplementation(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        Preconditions.CheckData(
            reader.TokenType == JsonTokenType.String,
            $"Unexpected token parsing instant. Expected String, got {reader.TokenType}."
        );

        var timeZoneId = reader.GetString()!;

        return _provider[timeZoneId];
    }

    /// <summary>
    /// Writes a DateTimeZone as its time zone ID string.
    /// </summary>
    /// <param name="writer">The JSON writer to write to.</param>
    /// <param name="value">The DateTimeZone to serialize.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void WriteImplementation(Utf8JsonWriter writer, DateTimeZone value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Id);
    }
}
