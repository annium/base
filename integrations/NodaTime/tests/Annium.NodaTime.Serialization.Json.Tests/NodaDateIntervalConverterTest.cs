using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

public class NodaDateIntervalConverterTest
{
    private readonly JsonConverter[] _converters = { Converters.DateIntervalConverter, Converters.LocalDateConverter };

    [Fact]
    public void RoundTrip()
    {
        var startLocalDate = new LocalDate(2012, 1, 2);
        var endLocalDate = new LocalDate(2013, 6, 7);
        var dateInterval = new DateInterval(startLocalDate, endLocalDate);
        AssertConversions(dateInterval, "{\"start\":\"2012-01-02\",\"end\":\"2013-06-07\"}", _converters);
    }

    [Fact]
    public void Serialize_InObject()
    {
        var startLocalDate = new LocalDate(2012, 1, 2);
        var endLocalDate = new LocalDate(2013, 6, 7);
        var dateInterval = new DateInterval(startLocalDate, endLocalDate);

        var testObject = new TestObject { Interval = dateInterval };

        var json = JsonSerializer.Serialize(testObject, With(_converters));
        json.Is("{\"interval\":{\"start\":\"2012-01-02\",\"end\":\"2013-06-07\"}}");
    }

    [Fact]
    public void Deserialize_InObject()
    {
        string json = "{\"interval\":{\"start\":\"2012-01-02\",\"end\":\"2013-06-07\"}}";

        var testObject = JsonSerializer.Deserialize<TestObject>(json, With(_converters))!;

        var interval = testObject.Interval;

        var startLocalDate = new LocalDate(2012, 1, 2);
        var endLocalDate = new LocalDate(2013, 6, 7);
        var expectedInterval = new DateInterval(startLocalDate, endLocalDate);
        interval.Is(expectedInterval);
    }

    public class TestObject
    {
        public DateInterval Interval { get; set; } = null!;
    }
}