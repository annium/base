using System.Text.Json;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

public class NodaInstantConverterTest
{
    [Fact]
    public void Serialize_NonNullableType()
    {
        var instant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var json = JsonSerializer.Serialize(instant, With(Converters.InstantConverter));
        json.Is("\"2012-01-02T03:04:05Z\"");
    }

    // [Fact]
    // public void Serialize_NullableType_NonNullValue()
    // {
    //     Instant? instant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
    //     var json = JsonSerializer.Serialize(instant, With(Converters.InstantConverter));
    //     json.Is("\"2012-01-02T03:04:05Z\"");
    // }

    // [Fact]
    // public void Serialize_NullableType_NullValue()
    // {
    //     Instant? instant = null;
    //     var json = JsonSerializer.Serialize(instant, With(Converters.InstantConverter));
    //     json.Is("null");
    // }

    [Fact]
    public void Deserialize_ToNonNullableType()
    {
        string json = "\"2012-01-02T03:04:05Z\"";
        var instant = JsonSerializer.Deserialize<Instant>(json, With(Converters.InstantConverter));
        instant.Is(Instant.FromUtc(2012, 1, 2, 3, 4, 5));
    }

    // [Fact]
    // public void Deserialize_ToNullableType_NonNullValue()
    // {
    //     string json = "\"2012-01-02T03:04:05Z\"";
    //     var instant = JsonSerializer.Deserialize<Instant?>(json, With(Converters.InstantConverter));
    //     instant.Is(Instant.FromUtc(2012, 1, 2, 3, 4, 5));
    // }

    // [Fact]
    // public void Deserialize_ToNullableType_NullValue()
    // {
    //     string json = "null";
    //     var instant = JsonSerializer.Deserialize<Instant?>(json, With(Converters.InstantConverter));
    //     instant.IsDefault();
    // }
}