using System.Text.Json;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// Tests for the Instant JSON converter that handles instant serialization.
/// </summary>
public class NodaInstantConverterTest
{
    /// <summary>
    /// Tests that non-nullable Instant values are correctly serialized to ISO-8601 format.
    /// </summary>
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

    /// <summary>
    /// Tests that ISO-8601 strings are correctly deserialized to non-nullable Instant values.
    /// </summary>
    [Fact]
    public void Deserialize_ToNonNullableType()
    {
        var json = "\"2012-01-02T03:04:05Z\"";
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
