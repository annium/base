using System.Text.Json;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// Tests for the JsonSerializerOptionsExtensions entry points: ConfigureForNodaTime,
/// WithIsoIntervalConverter, and WithIsoDateIntervalConverter.
/// </summary>
public class JsonSerializerOptionsExtensionsTest
{
    // -------------------------------------------------------------------------
    // ConfigureForNodaTime
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that ConfigureForNodaTime sets DictionaryKeyPolicy to CamelCase.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_SetsDictionaryKeyPolicyCamelCase()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        options.DictionaryKeyPolicy.Is(JsonNamingPolicy.CamelCase);
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime sets a non-null Encoder.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_SetsEncoder()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        options.Encoder.IsNotNull();
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime enables round-trip of Instant values.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_RoundTrip_Instant()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var value = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<Instant>(json, options);
        deserialized.Is(value);
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime enables round-trip of LocalDate values.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_RoundTrip_LocalDate()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var value = new LocalDate(2024, 6, 15);
        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<LocalDate>(json, options);
        deserialized.Is(value);
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime enables round-trip of ZonedDateTime values in UTC.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_RoundTrip_ZonedDateTimeUtc()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var zone = DateTimeZone.Utc;
        var value = new ZonedDateTime(Instant.FromUtc(2024, 6, 15, 10, 30, 0), zone);
        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<ZonedDateTime>(json, options);
        deserialized.Is(value);
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime enables round-trip of Interval values.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_RoundTrip_Interval()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var value = new Interval(Instant.FromUtc(2012, 1, 2, 3, 4, 5), Instant.FromUtc(2013, 6, 7, 8, 9, 10));
        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<Interval>(json, options);
        deserialized.Is(value);
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime enables round-trip of DateInterval values.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_RoundTrip_DateInterval()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var value = new DateInterval(new LocalDate(2012, 1, 2), new LocalDate(2013, 6, 7));
        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<DateInterval>(json, options);
        deserialized.Is(value);
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime enables round-trip of Duration values.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_RoundTrip_Duration()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var value = Duration.FromHours(48) + Duration.FromSeconds(3);
        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<Duration>(json, options);
        deserialized.Is(value);
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime enables round-trip of Period values.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_RoundTrip_Period()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var value = Period.FromDays(2) + Period.FromHours(4) + Period.FromMinutes(30);
        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<Period>(json, options);
        deserialized.Is(value);
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime enables round-trip of Offset values.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_RoundTrip_Offset()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var value = Offset.FromHoursAndMinutes(5, 30);
        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<Offset>(json, options);
        deserialized.Is(value);
    }

    /// <summary>
    /// Tests that ConfigureForNodaTime enables round-trip of YearMonth values.
    /// </summary>
    [Fact]
    public void ConfigureForNodaTime_RoundTrip_YearMonth()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime();
        var value = new YearMonth(2000, 2, CalendarSystem.Iso);
        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<YearMonth>(json, options);
        deserialized.Is(value);
    }

    // -------------------------------------------------------------------------
    // WithIsoIntervalConverter
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that WithIsoIntervalConverter causes Interval to serialize as an ISO slash string
    /// rather than the default JSON object with start/end properties.
    /// </summary>
    [Fact]
    public void WithIsoIntervalConverter_SerializesIntervalAsSlashString()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime().WithIsoIntervalConverter();
        var start = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var end = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var interval = new Interval(start, end);

        var json = JsonSerializer.Serialize(interval, options);

        // Must be a JSON string (starts with "), not a JSON object (starts with {)
        json.StartsWith("\"").IsTrue();
        json.Contains("/").IsTrue();
    }

    /// <summary>
    /// Tests that WithIsoIntervalConverter produces a round-trippable ISO slash string for Interval.
    /// </summary>
    [Fact]
    public void WithIsoIntervalConverter_RoundTrip()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime().WithIsoIntervalConverter();
        var start = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        var end = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
        var interval = new Interval(start, end);

        var json = JsonSerializer.Serialize(interval, options);
        var deserialized = JsonSerializer.Deserialize<Interval>(json, options);
        deserialized.Is(interval);
    }

    // -------------------------------------------------------------------------
    // WithIsoDateIntervalConverter
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that WithIsoDateIntervalConverter causes DateInterval to serialize as an ISO slash string
    /// rather than the default JSON object with start/end properties.
    /// </summary>
    [Fact]
    public void WithIsoDateIntervalConverter_SerializesDateIntervalAsSlashString()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime().WithIsoDateIntervalConverter();
        var dateInterval = new DateInterval(new LocalDate(2012, 1, 2), new LocalDate(2013, 6, 7));

        var json = JsonSerializer.Serialize(dateInterval, options);

        // Must be a JSON string (starts with "), not a JSON object (starts with {)
        json.StartsWith("\"").IsTrue();
        json.Contains("/").IsTrue();
    }

    /// <summary>
    /// Tests that WithIsoDateIntervalConverter produces a round-trippable ISO slash string for DateInterval.
    /// </summary>
    [Fact]
    public void WithIsoDateIntervalConverter_RoundTrip()
    {
        var options = new JsonSerializerOptions().ConfigureForNodaTime().WithIsoDateIntervalConverter();
        var dateInterval = new DateInterval(new LocalDate(2012, 1, 2), new LocalDate(2013, 6, 7));

        var json = JsonSerializer.Serialize(dateInterval, options);
        var deserialized = JsonSerializer.Deserialize<DateInterval>(json, options);
        deserialized.Is(dateInterval);
    }
}
