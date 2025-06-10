using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

/// <summary>
/// Tests for zoned date time extension methods that provide Unix time conversion, date/time operations, and period manipulations.
/// </summary>
public class ZonedDateTimeExtensionsTest
{
    /// <summary>
    /// Test moment representing a specific zoned date and time: November 26, 1971 at 10:40 AM UTC.
    /// </summary>
    private readonly ZonedDateTime _moment = new(
        new LocalDateTime(1971, 11, 26, 10, 40),
        DateTimeZone.Utc,
        Offset.Zero
    );

    /// <summary>
    /// Tests that FromUnixTimeMinutes correctly converts Unix time in minutes to a ZonedDateTime.
    /// </summary>
    [Fact]
    public void FromUnixTimeMinutes()
    {
        // arrange
        var value = ZonedDateTimeExtensions.FromUnixTimeMinutes(1_000_000L);

        // assert
        value.Is(_moment);
    }

    /// <summary>
    /// Tests that FromUnixTimeSeconds correctly converts Unix time in seconds to a ZonedDateTime.
    /// </summary>
    [Fact]
    public void FromUnixTimeSeconds()
    {
        // arrange
        var value = ZonedDateTimeExtensions.FromUnixTimeSeconds(60_000_000L);

        // assert
        value.Is(_moment);
    }

    /// <summary>
    /// Tests that FromUnixTimeMilliseconds correctly converts Unix time in milliseconds to a ZonedDateTime.
    /// </summary>
    [Fact]
    public void FromUnixTimeMilliseconds()
    {
        // arrange
        var value = ZonedDateTimeExtensions.FromUnixTimeMilliseconds(60_000_000_000L);

        // assert
        value.Is(_moment);
    }

    /// <summary>
    /// Tests that GetYearMonth extracts the year and month from a ZonedDateTime.
    /// </summary>
    [Fact]
    public void GetYearMonth()
    {
        // arrange
        var value = _moment.GetYearMonth();

        // assert
        value.Is(new YearMonth(_moment.Era, _moment.YearOfEra, _moment.Month, _moment.Calendar));
    }

    /// <summary>
    /// Tests that IsMidnight correctly identifies ZonedDateTime instances that represent midnight.
    /// </summary>
    [Fact]
    public void IsMidnight()
    {
        // arrange
        var mignight = new ZonedDateTime(new LocalDateTime(1, 2, 3, 0, 0, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var nonMignight = new ZonedDateTime(new LocalDateTime(1, 2, 3, 0, 0, 0, 1), DateTimeZone.Utc, Offset.Zero);

        // assert
        mignight.IsMidnight().IsTrue();
        nonMignight.IsMidnight().IsFalse();
    }

    /// <summary>
    /// Tests that ToUnixTimeMinutes correctly converts a ZonedDateTime to Unix time in minutes.
    /// </summary>
    [Fact]
    public void ToUnixTimeMinutes()
    {
        // arrange
        var value = _moment.ToUnixTimeMinutes();

        // assert
        value.Is(1_000_000L);
    }

    /// <summary>
    /// Tests that ToUnixTimeSeconds correctly converts a ZonedDateTime to Unix time in seconds.
    /// </summary>
    [Fact]
    public void ToUnixTimeSeconds()
    {
        // arrange
        var value = _moment.ToUnixTimeSeconds();

        // assert
        value.Is(60_000_000L);
    }

    /// <summary>
    /// Tests that ToUnixTimeMilliseconds correctly converts a ZonedDateTime to Unix time in milliseconds.
    /// </summary>
    [Fact]
    public void ToUnixTimeMilliseconds()
    {
        // arrange
        var value = _moment.ToUnixTimeMilliseconds();

        // assert
        value.Is(60_000_000_000L);
    }

    /// <summary>
    /// Tests that FloorToSecond floors a ZonedDateTime's period to the nearest second boundary below.
    /// </summary>
    [Fact]
    public void FloorToSecond()
    {
        Wrap(Period.FromMilliseconds(1999)).FloorToSecond().Is(Wrap(Period.FromSeconds(1)));
    }

    /// <summary>
    /// Tests that FloorToMinute floors a ZonedDateTime's period to the nearest minute boundary below.
    /// </summary>
    [Fact]
    public void FloorToMinute()
    {
        Wrap(Period.FromSeconds(100)).FloorToMinute().Is(Wrap(Period.FromMinutes(1)));
    }

    /// <summary>
    /// Tests that FloorToHour floors a ZonedDateTime's period to the nearest hour boundary below.
    /// </summary>
    [Fact]
    public void FloorToHour()
    {
        Wrap(Period.FromMinutes(100)).FloorToHour().Is(Wrap(Period.FromHours(1)));
    }

    /// <summary>
    /// Tests that FloorToDay floors a ZonedDateTime's period to the nearest day boundary below.
    /// </summary>
    [Fact]
    public void FloorToDay()
    {
        Wrap(Period.FromHours(30)).FloorToDay().Is(Wrap(Period.FromDays(1)));
    }

    /// <summary>
    /// Tests that FloorTo floors a ZonedDateTime's period to the nearest boundary of a specified period below.
    /// </summary>
    [Fact]
    public void FloorTo()
    {
        Wrap(Period.FromSeconds(55)).FloorTo(Period.FromSeconds(15)).Is(Wrap(Period.FromSeconds(45)));
    }

    /// <summary>
    /// Tests that CeilToSecond ceils a ZonedDateTime's period to the nearest second boundary above.
    /// </summary>
    [Fact]
    public void CeilToSecond()
    {
        Wrap(Period.FromMilliseconds(1)).CeilToSecond().Is(Wrap(Period.FromSeconds(1)));
    }

    /// <summary>
    /// Tests that CeilToMinute ceils a ZonedDateTime's period to the nearest minute boundary above.
    /// </summary>
    [Fact]
    public void CeilToMinute()
    {
        Wrap(Period.FromSeconds(1)).CeilToMinute().Is(Wrap(Period.FromMinutes(1)));
    }

    /// <summary>
    /// Tests that CeilToHour ceils a ZonedDateTime's period to the nearest hour boundary above.
    /// </summary>
    [Fact]
    public void CeilToHour()
    {
        Wrap(Period.FromMinutes(1)).CeilToHour().Is(Wrap(Period.FromHours(1)));
    }

    /// <summary>
    /// Tests that CeilToDay ceils a ZonedDateTime's period to the nearest day boundary above.
    /// </summary>
    [Fact]
    public void CeilToDay()
    {
        Wrap(Period.FromHours(1)).CeilToDay().Is(Wrap(Period.FromDays(1)));
    }

    /// <summary>
    /// Tests that CeilTo ceils a ZonedDateTime's period to the nearest boundary of a specified period above.
    /// </summary>
    [Fact]
    public void CeilTo()
    {
        Wrap(Period.FromSeconds(55)).CeilTo(Period.FromSeconds(15)).Is(Wrap(Period.FromSeconds(60)));
    }

    /// <summary>
    /// Tests that RoundToSecond rounds a ZonedDateTime's period to the nearest second boundary.
    /// </summary>
    [Fact]
    public void RoundToSecond()
    {
        Wrap(Period.FromMilliseconds(499)).RoundToSecond().Is(Wrap(Period.Zero));
        Wrap(Period.FromMilliseconds(500)).RoundToSecond().Is(Wrap(Period.FromSeconds(1)));
    }

    /// <summary>
    /// Tests that RoundToMinute rounds a ZonedDateTime's period to the nearest minute boundary.
    /// </summary>
    [Fact]
    public void RoundToMinute()
    {
        Wrap(Period.FromSeconds(29)).RoundToMinute().Is(Wrap(Period.Zero));
        Wrap(Period.FromSeconds(30)).RoundToMinute().Is(Wrap(Period.FromMinutes(1)));
    }

    /// <summary>
    /// Tests that RoundToHour rounds a ZonedDateTime's period to the nearest hour boundary.
    /// </summary>
    [Fact]
    public void RoundToHour()
    {
        Wrap(Period.FromMinutes(29)).RoundToHour().Is(Wrap(Period.Zero));
        Wrap(Period.FromMinutes(30)).RoundToHour().Is(Wrap(Period.FromHours(1)));
    }

    /// <summary>
    /// Tests that RoundToDay rounds a ZonedDateTime's period to the nearest day boundary.
    /// </summary>
    [Fact]
    public void RoundToDay()
    {
        Wrap(Period.FromHours(11)).RoundToDay().Is(Wrap(Period.Zero));
        Wrap(Period.FromHours(12)).RoundToDay().Is(Wrap(Period.FromDays(1)));
    }

    /// <summary>
    /// Tests that RoundTo rounds a ZonedDateTime's period to the nearest boundary of a specified period.
    /// </summary>
    [Fact]
    public void RoundTo()
    {
        Wrap(Period.FromSeconds(50)).RoundTo(Period.FromSeconds(15)).Is(Wrap(Period.FromSeconds(45)));
        Wrap(Period.FromSeconds(55)).RoundTo(Period.FromSeconds(15)).Is(Wrap(Period.FromSeconds(60)));
    }

    /// <summary>
    /// Helper method to convert a Period to a ZonedDateTime by adding it to Instant.MinValue in UTC.
    /// </summary>
    /// <param name="period">The period to convert.</param>
    /// <returns>A ZonedDateTime representing the period offset from minimum instant in UTC timezone.</returns>
    private static ZonedDateTime Wrap(Period period) => new(Instant.MinValue + period.ToDuration(), DateTimeZone.Utc);
}
