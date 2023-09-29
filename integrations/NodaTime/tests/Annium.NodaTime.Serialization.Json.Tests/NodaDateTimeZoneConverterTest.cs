using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

public class NodaDateTimeZoneConverterTest
{
    private readonly JsonConverter _converter = Converters.CreateDateTimeZoneConverter(DateTimeZoneProviders.Tzdb);

    [Fact]
    public void Serialize()
    {
        var dateTimeZone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        var json = JsonSerializer.Serialize(dateTimeZone, With(_converter));
        json.Is("\"America/Los_Angeles\"");
    }

    [Fact]
    public void Deserialize()
    {
        string json = "\"America/Los_Angeles\"";
        var dateTimeZone = JsonSerializer.Deserialize<DateTimeZone>(json, With(_converter));
        var expectedDateTimeZone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        dateTimeZone.Is(expectedDateTimeZone);
    }

    [Fact]
    public void Deserialize_TimeZoneNotFound()
    {
        string json = "\"America/DOES_NOT_EXIST\"";
        Wrap.It(() => JsonSerializer.Deserialize<DateTimeZone>(json, With(_converter)))
            .Throws<JsonException>();
    }
}