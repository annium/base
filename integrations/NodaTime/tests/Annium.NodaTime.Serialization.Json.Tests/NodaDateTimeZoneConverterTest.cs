using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// Tests for the DateTimeZone JSON converter that handles time zone serialization.
/// </summary>
public class NodaDateTimeZoneConverterTest
{
    /// <summary>
    /// JSON converter for DateTimeZone using the TZDB provider.
    /// </summary>
    private readonly JsonConverter _converter = Converters.CreateDateTimeZoneConverter(DateTimeZoneProviders.Tzdb);

    /// <summary>
    /// Tests that DateTimeZone values are correctly serialized to their ID strings.
    /// </summary>
    [Fact]
    public void Serialize()
    {
        var dateTimeZone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        var json = JsonSerializer.Serialize(dateTimeZone, With(_converter));
        json.Is("\"America/Los_Angeles\"");
    }

    /// <summary>
    /// Tests that DateTimeZone values are correctly deserialized from their ID strings.
    /// </summary>
    [Fact]
    public void Deserialize()
    {
        var json = "\"America/Los_Angeles\"";
        var dateTimeZone = JsonSerializer.Deserialize<DateTimeZone>(json, With(_converter));
        var expectedDateTimeZone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        dateTimeZone.Is(expectedDateTimeZone);
    }

    /// <summary>
    /// Tests that deserializing an invalid time zone ID throws a JsonException.
    /// </summary>
    [Fact]
    public void Deserialize_TimeZoneNotFound()
    {
        var json = "\"America/DOES_NOT_EXIST\"";
        Wrap.It(() => JsonSerializer.Deserialize<DateTimeZone>(json, With(_converter))).Throws<JsonException>();
    }
}
