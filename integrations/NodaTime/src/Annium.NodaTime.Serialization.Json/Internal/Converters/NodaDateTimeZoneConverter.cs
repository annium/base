using System;
using System.Text.Json;
using NodaTime;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Json converter for <see cref="DateTimeZone"/>.
/// </summary>
internal sealed class NodaDateTimeZoneConverter : ConverterBase<DateTimeZone>
{
    private readonly IDateTimeZoneProvider _provider;

    /// <param name="provider">Provides the <see cref="DateTimeZone"/> that corresponds to each time zone ID in the JSON string.</param>
    public NodaDateTimeZoneConverter(IDateTimeZoneProvider provider)
    {
        _provider = provider;
    }

    public override DateTimeZone ReadImplementation(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        Preconditions.CheckData(reader.TokenType == JsonTokenType.String, $"Unexpected token parsing instant. Expected String, got {reader.TokenType}.");

        var timeZoneId = reader.GetString()!;

        return _provider[timeZoneId];
    }

    public override void WriteImplementation(
        Utf8JsonWriter writer,
        DateTimeZone value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStringValue(value.Id);
    }
}