using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

/// <summary>
/// Tests for local date time extension methods that provide Unix time conversion, date/time operations, and period manipulations.
/// </summary>
public class LocalDateTimeExtensionsTest
{
    /// <summary>
    /// Test moment representing a specific date and time: November 26, 1971 at 10:40 AM.
    /// </summary>
    private readonly LocalDateTime _moment = new(1971, 11, 26, 10, 40);

    /// <summary>
    /// Tests that FromUnixTimeMinutes correctly converts Unix time in minutes to a LocalDateTime.
    /// </summary>
    [Fact]
    public void FromUnixTimeMinutes()
    {
        // arrange
        var value = LocalDateTimeExtensions.FromUnixTimeMinutes(1_000_000L);

        // assert
        value.Is(_moment);
    }

    /// <summary>
    /// Tests that FromUnixTimeSeconds correctly converts Unix time in seconds to a LocalDateTime.
    /// </summary>
    [Fact]
    public void FromUnixTimeSeconds()
    {
        // arrange
        var value = LocalDateTimeExtensions.FromUnixTimeSeconds(60_000_000L);

        // assert
        value.Is(_moment);
    }

    /// <summary>
    /// Tests that FromUnixTimeMilliseconds correctly converts Unix time in milliseconds to a LocalDateTime.
    /// </summary>
    [Fact]
    public void FromUnixTimeMilliseconds()
    {
        // arrange
        var value = LocalDateTimeExtensions.FromUnixTimeMilliseconds(60_000_000_000L);

        // assert
        value.Is(_moment);
    }

    /// <summary>
    /// Tests that GetYearMonth extracts the year and month from a LocalDateTime.
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
    /// Tests that IsMidnight correctly identifies LocalDateTime instances that represent midnight.
    /// </summary>
    [Fact]
    public void IsMidnight()
    {
        // arrange
        var mignight = new LocalDateTime(1, 2, 3, 0, 0, 0, 0);
        var nonMignight = new LocalDateTime(1, 2, 3, 0, 0, 0, 1);

        // assert
        mignight.IsMidnight().IsTrue();
        nonMignight.IsMidnight().IsFalse();
    }

    /// <summary>
    /// Tests that ToUnixTimeMinutes correctly converts a LocalDateTime to Unix time in minutes.
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
    /// Tests that ToUnixTimeSeconds correctly converts a LocalDateTime to Unix time in seconds.
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
    /// Tests that ToUnixTimeMilliseconds correctly converts a LocalDateTime to Unix time in milliseconds.
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
    /// Tests that FloorToSecond floors a LocalDateTime's period to the nearest second boundary below.
    /// </summary>
    [Fact]
    public void FloorToSecond()
    {
        Wrap(Period.FromMilliseconds(1999)).FloorToSecond().Is(Wrap(Period.FromSeconds(1)));
    }

    /// <summary>
    /// Tests that FloorToMinute floors a LocalDateTime's period to the nearest minute boundary below.
    /// </summary>
    [Fact]
    public void FloorToMinute()
    {
        Wrap(Period.FromSeconds(100)).FloorToMinute().Is(Wrap(Period.FromMinutes(1)));
    }

    /// <summary>
    /// Tests that FloorToHour floors a LocalDateTime's period to the nearest hour boundary below.
    /// </summary>
    [Fact]
    public void FloorToHour()
    {
        Wrap(Period.FromMinutes(100)).FloorToHour().Is(Wrap(Period.FromHours(1)));
    }

    /// <summary>
    /// Tests that FloorToDay floors a LocalDateTime's period to the nearest day boundary below.
    /// </summary>
    [Fact]
    public void FloorToDay()
    {
        Wrap(Period.FromHours(30)).FloorToDay().Is(Wrap(Period.FromDays(1)));
    }

    /// <summary>
    /// Tests that FloorTo floors a LocalDateTime's period to the nearest boundary of a specified period below.
    /// </summary>
    [Fact]
    public void FloorTo()
    {
        Wrap(Period.FromSeconds(55)).FloorTo(Period.FromSeconds(15)).Is(Wrap(Period.FromSeconds(45)));
    }

    /// <summary>
    /// Tests that CeilToSecond ceils a LocalDateTime's period to the nearest second boundary above.
    /// </summary>
    [Fact]
    public void CeilToSecond()
    {
        Wrap(Period.FromMilliseconds(1)).CeilToSecond().Is(Wrap(Period.FromSeconds(1)));
    }

    /// <summary>
    /// Tests that CeilToMinute ceils a LocalDateTime's period to the nearest minute boundary above.
    /// </summary>
    [Fact]
    public void CeilToMinute()
    {
        Wrap(Period.FromSeconds(1)).CeilToMinute().Is(Wrap(Period.FromMinutes(1)));
    }

    /// <summary>
    /// Tests that CeilToHour ceils a LocalDateTime's period to the nearest hour boundary above.
    /// </summary>
    [Fact]
    public void CeilToHour()
    {
        Wrap(Period.FromMinutes(1)).CeilToHour().Is(Wrap(Period.FromHours(1)));
    }

    /// <summary>
    /// Tests that CeilToDay ceils a LocalDateTime's period to the nearest day boundary above.
    /// </summary>
    [Fact]
    public void CeilToDay()
    {
        Wrap(Period.FromHours(1)).CeilToDay().Is(Wrap(Period.FromDays(1)));
    }

    /// <summary>
    /// Tests that CeilTo ceils a LocalDateTime's period to the nearest boundary of a specified period above.
    /// </summary>
    [Fact]
    public void CeilTo()
    {
        Wrap(Period.FromSeconds(55)).CeilTo(Period.FromSeconds(15)).Is(Wrap(Period.FromSeconds(60)));
    }

    /// <summary>
    /// Tests that RoundToSecond rounds a LocalDateTime's period to the nearest second boundary.
    /// </summary>
    [Fact]
    public void RoundToSecond()
    {
        Wrap(Period.FromMilliseconds(499)).RoundToSecond().Is(Wrap(Period.Zero));
        Wrap(Period.FromMilliseconds(500)).RoundToSecond().Is(Wrap(Period.FromSeconds(1)));
    }

    /// <summary>
    /// Tests that RoundToMinute rounds a LocalDateTime's period to the nearest minute boundary.
    /// </summary>
    [Fact]
    public void RoundToMinute()
    {
        Wrap(Period.FromSeconds(29)).RoundToMinute().Is(Wrap(Period.Zero));
        Wrap(Period.FromSeconds(30)).RoundToMinute().Is(Wrap(Period.FromMinutes(1)));
    }

    /// <summary>
    /// Tests that RoundToHour rounds a LocalDateTime's period to the nearest hour boundary.
    /// </summary>
    [Fact]
    public void RoundToHour()
    {
        Wrap(Period.FromMinutes(29)).RoundToHour().Is(Wrap(Period.Zero));
        Wrap(Period.FromMinutes(30)).RoundToHour().Is(Wrap(Period.FromHours(1)));
    }

    /// <summary>
    /// Tests that RoundToDay rounds a LocalDateTime's period to the nearest day boundary.
    /// </summary>
    [Fact]
    public void RoundToDay()
    {
        Wrap(Period.FromHours(11)).RoundToDay().Is(Wrap(Period.Zero));
        Wrap(Period.FromHours(12)).RoundToDay().Is(Wrap(Period.FromDays(1)));
    }

    /// <summary>
    /// Tests that RoundTo rounds a LocalDateTime's period to the nearest boundary of a specified period.
    /// </summary>
    [Fact]
    public void RoundTo()
    {
        Wrap(Period.FromSeconds(50)).RoundTo(Period.FromSeconds(15)).Is(Wrap(Period.FromSeconds(45)));
        Wrap(Period.FromSeconds(55)).RoundTo(Period.FromSeconds(15)).Is(Wrap(Period.FromSeconds(60)));
    }

    /// <summary>
    /// Helper method to convert a Period to a LocalDateTime by adding it to Instant.MinValue and converting to UTC.
    /// </summary>
    /// <param name="period">The period to convert.</param>
    /// <returns>A LocalDateTime representing the period offset from minimum instant in UTC.</returns>
    private static LocalDateTime Wrap(Period period) => (Instant.MinValue + period.ToDuration()).InUtc().LocalDateTime;
}
