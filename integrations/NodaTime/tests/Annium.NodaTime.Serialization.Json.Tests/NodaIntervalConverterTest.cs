using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

public class NodaIntervalConverterTest
{
    private readonly JsonConverter[] _converters = { Converters.IntervalConverter, Converters.InstantConverter };

    [Fact]
    public void RoundTrip()
    {
        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5) + Duration.FromMilliseconds(670);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10) + Duration.FromNanoseconds(123456789);
        var interval = new Interval(startInstant, endInstant);
        AssertConversions(interval, "{\"start\":\"2012-01-02T03:04:05.67Z\",\"end\":\"2013-06-07T08:09:10.123456789Z\"}", _converters);
    }

    [Fact]
    public void RoundTrip_Infinite()
    {
        var instant = Instant.FromUtc(2013, 6, 7, 8, 9, 10) + Duration.FromNanoseconds(123456789);
        AssertConversions(new Interval(null, instant), "{\"end\":\"2013-06-07T08:09:10.123456789Z\"}", _converters);
        AssertConversions(new Interval(instant, null), "{\"start\":\"2013-06-07T08:09:10.123456789Z\"}", _converters);
        AssertConversions(new Interval(null, null), "{}", _converters);
    }

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

    [Fact]
    public void Deserialize_InObject()
    {
        string json = "{\"interval\":{\"start\":\"2012-01-02T03:04:05Z\",\"end\":\"2013-06-07T08:09:10Z\"}}";

        var testObject = JsonSerializer.Deserialize<TestObject>(json, With(_converters))!;

        var interval = testObject.Interval;

        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var expectedInterval = new Interval(startInstant, endInstant);
        interval.Is(expectedInterval);
    }

    [Fact]
    public void Deserialize_InObject_CamelCase()
    {
        string json = "{\"interval\":{\"start\":\"2012-01-02T03:04:05Z\",\"end\":\"2013-06-07T08:09:10Z\"}}";

        var testObject = JsonSerializer.Deserialize<TestObject>(json, With(_converters))!;

        var interval = testObject.Interval;

        var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var expectedInterval = new Interval(startInstant, endInstant);
        interval.Is(expectedInterval);
    }

    public class TestObject
    {
        public Interval Interval { get; set; }
    }
}