using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// Tests for the DateInterval JSON converter that handles serialization of date intervals.
/// </summary>
public class NodaDateIntervalConverterTest
{
    /// <summary>
    /// JSON converters used for testing DateInterval serialization.
    /// </summary>
    private readonly JsonConverter[] _converters = { Converters.DateIntervalConverter, Converters.LocalDateConverter };

    /// <summary>
    /// Tests that DateInterval values can be serialized to JSON and deserialized back correctly.
    /// </summary>
    [Fact]
    public void RoundTrip()
    {
        var startLocalDate = new LocalDate(2012, 1, 2);
        var endLocalDate = new LocalDate(2013, 6, 7);
        var dateInterval = new DateInterval(startLocalDate, endLocalDate);
        AssertConversions(dateInterval, "{\"start\":\"2012-01-02\",\"end\":\"2013-06-07\"}", _converters);
    }

    /// <summary>
    /// Tests that DateInterval values can be serialized when contained within another object.
    /// </summary>
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

    /// <summary>
    /// Tests that DateInterval values can be deserialized when contained within another object.
    /// </summary>
    [Fact]
    public void Deserialize_InObject()
    {
        var json = "{\"interval\":{\"start\":\"2012-01-02\",\"end\":\"2013-06-07\"}}";

        var testObject = JsonSerializer.Deserialize<TestObject>(json, With(_converters))!;

        var interval = testObject.Interval;

        var startLocalDate = new LocalDate(2012, 1, 2);
        var endLocalDate = new LocalDate(2013, 6, 7);
        var expectedInterval = new DateInterval(startLocalDate, endLocalDate);
        interval.Is(expectedInterval);
    }

    /// <summary>
    /// Test object containing a DateInterval property for testing serialization scenarios.
    /// </summary>
    public class TestObject
    {
        /// <summary>
        /// Gets or sets the date interval for testing.
        /// </summary>
        public DateInterval Interval { get; set; } = null!;
    }
}
