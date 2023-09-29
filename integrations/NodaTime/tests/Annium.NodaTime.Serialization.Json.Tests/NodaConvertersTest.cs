using System;
using System.Text.Json;
using Annium.Testing;
using NodaTime;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// Tests for the converters exposed in Converters.
/// </summary>
public class ConvertersTest
{
    [Fact]
    public void OffsetConverter()
    {
        var value = Offset.FromHoursAndMinutes(5, 30);
        string json = "\"+05:30\"";
        AssertConversions(value, json, Converters.OffsetConverter);
    }

    [Fact]
    public void InstantConverter()
    {
        var value = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
        string json = "\"2012-01-02T03:04:05Z\"";
        AssertConversions(value, json, Converters.InstantConverter);
    }

    [Fact]
    public void LocalDateConverter()
    {
        var value = new LocalDate(2012, 1, 2, CalendarSystem.Iso);
        string json = "\"2012-01-02\"";
        AssertConversions(value, json, Converters.LocalDateConverter);
    }

    [Fact]
    public void LocalDateConverter_SerializeNonIso_Throws()
    {
        var localDate = new LocalDate(2012, 1, 2, CalendarSystem.Coptic);

        Wrap.It(() => JsonSerializer.Serialize(localDate, With(Converters.LocalDateConverter))).Throws<ArgumentException>();
    }

    [Fact]
    public void LocalDateTimeConverter()
    {
        var value = new LocalDateTime(2012, 1, 2, 3, 4, 5, CalendarSystem.Iso).PlusNanoseconds(123456789);
        var json = "\"2012-01-02T03:04:05.123456789\"";
        AssertConversions(value, json, Converters.LocalDateTimeConverter);
    }

    [Fact]
    public void LocalDateTimeConverter_SerializeNonIso_Throws()
    {
        var localDateTime = new LocalDateTime(2012, 1, 2, 3, 4, 5, CalendarSystem.Coptic);

        Wrap.It(() => JsonSerializer.Serialize(localDateTime, With(Converters.LocalDateTimeConverter))).Throws<ArgumentException>();
    }

    [Fact]
    public void LocalTimeConverter()
    {
        var value = LocalTime.FromHourMinuteSecondMillisecondTick(1, 2, 3, 4, 5).PlusNanoseconds(67);
        var json = "\"01:02:03.004000567\"";
        AssertConversions(value, json, Converters.LocalTimeConverter);
    }

    [Fact]
    public void RoundtripPeriodConverter()
    {
        var value = Period.FromDays(2) + Period.FromHours(3) + Period.FromMinutes(90);
        string json = "\"P2DT3H90M\"";
        AssertConversions(value, json, Converters.RoundtripPeriodConverter);
    }

    [Fact]
    public void NormalizingIsoPeriodConverter_RequiresNormalization()
    {
        // Can't use AssertConversions here, as it doesn't round-trip
        var period = Period.FromDays(2) + Period.FromHours(3) + Period.FromMinutes(90);
        var json = JsonSerializer.Serialize(period, With(Converters.NormalizingIsoPeriodConverter));
        json.Is("\"P2DT4H30M\"");
    }

    [Fact]
    public void NormalizingIsoPeriodConverter_AlreadyNormalized()
    {
        // This time we're okay as it's already a normalized value.
        var value = Period.FromDays(2) + Period.FromHours(4) + Period.FromMinutes(30);
        string json = "\"P2DT4H30M\"";
        AssertConversions(value, json, Converters.NormalizingIsoPeriodConverter);
    }

    [Fact]
    public void ZonedDateTimeConverter()
    {
        // Deliberately give it an ambiguous local time, in both ways.
        var zone = DateTimeZoneProviders.Tzdb["Europe/London"];
        var earlierValue = new ZonedDateTime(new LocalDateTime(2012, 10, 28, 1, 30), zone, Offset.FromHours(1));
        var laterValue = new ZonedDateTime(new LocalDateTime(2012, 10, 28, 1, 30), zone, Offset.FromHours(0));
        string earlierJson = "\"2012-10-28T01:30:00+01 Europe/London\"";
        string laterJson = "\"2012-10-28T01:30:00Z Europe/London\"";
        var converter = Converters.CreateZonedDateTimeConverter(DateTimeZoneProviders.Tzdb);

        AssertConversions(earlierValue, earlierJson, converter);
        AssertConversions(laterValue, laterJson, converter);
    }

    [Fact]
    public void OffsetDateTimeConverter()
    {
        var value = new LocalDateTime(2012, 1, 2, 3, 4, 5).PlusNanoseconds(123456789).WithOffset(Offset.FromHoursAndMinutes(-1, -30));
        string json = "\"2012-01-02T03:04:05.123456789-01:30\"";
        AssertConversions(value, json, Converters.OffsetDateTimeConverter);
    }

    [Fact]
    public void OffsetDateTimeConverter_WholeHours()
    {
        // Redundantly specify the minutes, so that Javascript can parse it and it's RFC3339-compliant.
        // See issue 284 for details.
        var value = new LocalDateTime(2012, 1, 2, 3, 4, 5).PlusNanoseconds(123456789).WithOffset(Offset.FromHours(5));
        string json = "\"2012-01-02T03:04:05.123456789+05:00\"";
        AssertConversions(value, json, Converters.OffsetDateTimeConverter);
    }

    [Fact]
    public void OffsetDateTimeConverter_ZeroOffset()
    {
        // Redundantly specify the minutes, so that Javascript can parse it and it's RFC3339-compliant.
        // See issue 284 for details.
        var value = new LocalDateTime(2012, 1, 2, 3, 4, 5).PlusNanoseconds(123456789).WithOffset(Offset.Zero);
        string json = "\"2012-01-02T03:04:05.123456789Z\"";
        AssertConversions(value, json, Converters.OffsetDateTimeConverter);
    }

    [Fact]
    public void Duration_WholeSeconds()
    {
        AssertConversions(Duration.FromHours(48), "\"48:00:00\"", Converters.DurationConverter);
    }

    [Fact]
    public void Duration_FractionalSeconds()
    {
        AssertConversions(Duration.FromHours(48) + Duration.FromSeconds(3) + Duration.FromNanoseconds(123456789), "\"48:00:03.123456789\"",
            Converters.DurationConverter);
        AssertConversions(Duration.FromHours(48) + Duration.FromSeconds(3) + Duration.FromTicks(1230000), "\"48:00:03.123\"", Converters.DurationConverter);
        AssertConversions(Duration.FromHours(48) + Duration.FromSeconds(3) + Duration.FromTicks(1234000), "\"48:00:03.1234\"",
            Converters.DurationConverter);
        AssertConversions(Duration.FromHours(48) + Duration.FromSeconds(3) + Duration.FromTicks(12345), "\"48:00:03.0012345\"",
            Converters.DurationConverter);
    }

    [Fact]
    public void Duration_MinAndMaxValues()
    {
        AssertConversions(Duration.FromTicks(long.MaxValue), "\"256204778:48:05.4775807\"", Converters.DurationConverter);
        AssertConversions(Duration.FromTicks(long.MinValue), "\"-256204778:48:05.4775808\"", Converters.DurationConverter);
    }

    /// <summary>
    /// The pre-release converter used either 3 or 7 decimal places for fractions of a second; never less.
    /// This test checks that the "new" converter (using DurationPattern) can still parse the old output.
    /// </summary>
    [Fact]
    public void Duration_ParsePartialFractionalSecondsWithTrailingZeroes()
    {
        var parsed = JsonSerializer.Deserialize<Duration>("\"25:10:00.1234000\"", With(Converters.DurationConverter));
        (Duration.FromHours(25) + Duration.FromMinutes(10) + Duration.FromTicks(1234000)).Is(parsed);
    }

    [Fact]
    public void OffsetDateConverter()
    {
        var value = new LocalDate(2012, 1, 2).WithOffset(Offset.FromHoursAndMinutes(-1, -30));
        string json = "\"2012-01-02-01:30\"";
        AssertConversions(value, json, Converters.OffsetDateConverter);
    }

    [Fact]
    public void OffsetTimeConverter()
    {
        var value = new LocalTime(3, 4, 5).PlusNanoseconds(123456789).WithOffset(Offset.FromHoursAndMinutes(-1, -30));
        string json = "\"03:04:05.123456789-01:30\"";
        AssertConversions(value, json, Converters.OffsetTimeConverter);
    }

    [Fact]
    public void YearMonthConverter()
    {
        var value = new YearMonth(2000, 2, CalendarSystem.Iso);
        string json = "\"2000-02\"";
        AssertConversions(value, json, Converters.YearMonthConverter);
    }

    [Fact]
    public void YearMonthConverter_SerializeNonIso_Throws()
    {
        var value = new YearMonth(2000, 2, CalendarSystem.Coptic);

        Wrap.It(() => JsonSerializer.Serialize(value, With(Converters.YearMonthConverter))).Throws<ArgumentException>();
    }
}