using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// The same tests as NodaIntervalConverterTest, but using the ISO-based interval converter.
/// </summary>
public class NodaIsoIntervalConverterTest
{
    /// <summary>
    /// JSON converters used for testing ISO interval serialization.
    /// </summary>
    private readonly JsonConverter[] _converters = { Converters.IsoIntervalConverter };

    /// <summary>
    /// Tests that Interval values can be serialized to ISO format and deserialized back correctly.
    /// </summary>
    [Fact]
    public void RoundTrip()
    {
        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5) + Duration.FromMilliseconds(670);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10) + Duration.FromNanoseconds(123456789);
        var interval = new Interval(startInstant, endInstant);
        AssertConversions(interval, "\"2012-01-02T03:04:05.67Z/2013-06-07T08:09:10.123456789Z\"", _converters);
    }

    /// <summary>
    /// Tests that infinite intervals are correctly serialized to ISO format with slash delimiters.
    /// </summary>
    [Fact]
    public void RoundTrip_Infinite()
    {
        var instant = Instant.FromUtc(2013, 6, 7, 8, 9, 10) + Duration.FromNanoseconds(123456789);
        AssertConversions(new Interval(null, instant), "\"/2013-06-07T08:09:10.123456789Z\"", _converters);
        AssertConversions(new Interval(instant, null), "\"2013-06-07T08:09:10.123456789Z/\"", _converters);
        AssertConversions(new Interval(null, null), "\"/\"", _converters);
    }

    /// <summary>
    /// Tests that ISO intervals with comma decimal separators can be correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeComma()
    {
        // Comma is deliberate, to show that we can parse a comma decimal separator too.
        var json = "\"2012-01-02T03:04:05.670Z/2013-06-07T08:09:10,1234567Z\"";

        var interval = JsonSerializer.Deserialize<Interval>(json, With(_converters));

        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5) + Duration.FromMilliseconds(670);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10) + Duration.FromTicks(1234567);
        var expectedInterval = new Interval(startInstant, endInstant);
        interval.Is(expectedInterval);
    }

    /// <summary>
    /// Tests that Interval values can be serialized in ISO format when contained within another object.
    /// </summary>
    [Fact]
    public void Serialize_InObject()
    {
        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var interval = new Interval(startInstant, endInstant);

        var testObject = new TestObject { Interval = interval };

        var json = JsonSerializer.Serialize(testObject, With(_converters));
        json.Is("{\"interval\":\"2012-01-02T03:04:05Z/2013-06-07T08:09:10Z\"}");
    }

    /// <summary>
    /// Tests that Interval values can be deserialized from ISO format when contained within another object.
    /// </summary>
    [Fact]
    public void Deserialize_InObject()
    {
        var json = "{\"interval\":\"2012-01-02T03:04:05Z/2013-06-07T08:09:10Z\"}";

        var testObject = JsonSerializer.Deserialize<TestObject>(json, With(_converters))!;

        var interval = testObject.Interval;

        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var expectedInterval = new Interval(startInstant, endInstant);
        interval.Is(expectedInterval);
    }

    /// <summary>
    /// Test object containing an Interval property for testing ISO serialization scenarios.
    /// </summary>
    public class TestObject
    {
        /// <summary>
        /// Gets or sets the interval for testing.
        /// </summary>
        public Interval Interval { get; set; }
    }
}
