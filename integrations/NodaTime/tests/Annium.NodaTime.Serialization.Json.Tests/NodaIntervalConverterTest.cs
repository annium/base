using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// Tests for the Interval JSON converter that handles time interval serialization.
/// </summary>
public class NodaIntervalConverterTest
{
    /// <summary>
    /// JSON converters used for testing Interval serialization.
    /// </summary>
    private readonly JsonConverter[] _converters = { Converters.IntervalConverter, Converters.InstantConverter };

    /// <summary>
    /// Tests that Interval values can be serialized to JSON and deserialized back correctly.
    /// </summary>
    [Fact]
    public void RoundTrip()
    {
        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5) + Duration.FromMilliseconds(670);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10) + Duration.FromNanoseconds(123456789);
        var interval = new Interval(startInstant, endInstant);
        AssertConversions(
            interval,
            "{\"start\":\"2012-01-02T03:04:05.67Z\",\"end\":\"2013-06-07T08:09:10.123456789Z\"}",
            _converters
        );
    }

    /// <summary>
    /// Tests that infinite intervals (with null start or end) are correctly serialized and deserialized.
    /// </summary>
    [Fact]
    public void RoundTrip_Infinite()
    {
        var instant = Instant.FromUtc(2013, 6, 7, 8, 9, 10) + Duration.FromNanoseconds(123456789);
        AssertConversions(new Interval(null, instant), "{\"end\":\"2013-06-07T08:09:10.123456789Z\"}", _converters);
        AssertConversions(new Interval(instant, null), "{\"start\":\"2013-06-07T08:09:10.123456789Z\"}", _converters);
        AssertConversions(new Interval(null, null), "{}", _converters);
    }

    /// <summary>
    /// Tests that Interval values can be serialized when contained within another object.
    /// </summary>
    [Fact]
    public void Serialize_InObject()
    {
        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var interval = new Interval(startInstant, endInstant);

        var testObject = new TestObject { Interval = interval };

        var json = JsonSerializer.Serialize(testObject, With(_converters));
        json.Is("{\"interval\":{\"start\":\"2012-01-02T03:04:05Z\",\"end\":\"2013-06-07T08:09:10Z\"}}");
    }

    /// <summary>
    /// Tests that Interval values can be serialized with camel case naming policy.
    /// </summary>
    [Fact]
    public void Serialize_InObject_CamelCase()
    {
        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var interval = new Interval(startInstant, endInstant);

        var testObject = new TestObject { Interval = interval };

        var json = JsonSerializer.Serialize(testObject, With(_converters));
        json.Is("{\"interval\":{\"start\":\"2012-01-02T03:04:05Z\",\"end\":\"2013-06-07T08:09:10Z\"}}");
    }

    /// <summary>
    /// Tests that Interval values can be deserialized when contained within another object.
    /// </summary>
    [Fact]
    public void Deserialize_InObject()
    {
        var json = "{\"interval\":{\"start\":\"2012-01-02T03:04:05Z\",\"end\":\"2013-06-07T08:09:10Z\"}}";

        var testObject = JsonSerializer.Deserialize<TestObject>(json, With(_converters))!;

        var interval = testObject.Interval;

        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var expectedInterval = new Interval(startInstant, endInstant);
        interval.Is(expectedInterval);
    }

    /// <summary>
    /// Tests that Interval values can be deserialized with camel case naming policy.
    /// </summary>
    [Fact]
    public void Deserialize_InObject_CamelCase()
    {
        var json = "{\"interval\":{\"start\":\"2012-01-02T03:04:05Z\",\"end\":\"2013-06-07T08:09:10Z\"}}";

        var testObject = JsonSerializer.Deserialize<TestObject>(json, With(_converters))!;

        var interval = testObject.Interval;

        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var expectedInterval = new Interval(startInstant, endInstant);
        interval.Is(expectedInterval);
    }

    /// <summary>
    /// Test object containing an Interval property for testing serialization scenarios.
    /// </summary>
    public class TestObject
    {
        /// <summary>
        /// Gets or sets the interval for testing.
        /// </summary>
        public Interval Interval { get; set; }
    }
}
