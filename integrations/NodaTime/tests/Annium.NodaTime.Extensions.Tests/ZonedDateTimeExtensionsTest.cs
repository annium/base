using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class ZonedDateTimeExtensionsTest
{
    private readonly ZonedDateTime _moment = new(new LocalDateTime(1971, 11, 26, 10, 40), DateTimeZone.Utc, Offset.Zero);

    [Fact]
    public void FromUnixTimeMinutes()
    {
        // arrange
        var value = ZonedDateTimeExtensions.FromUnixTimeMinutes(1_000_000L);

        // assert
        value.Is(_moment);
    }

    [Fact]
    public void FromUnixTimeSeconds()
    {
        // arrange
        var value = ZonedDateTimeExtensions.FromUnixTimeSeconds(60_000_000L);

        // assert
        value.Is(_moment);
    }

    [Fact]
    public void FromUnixTimeMilliseconds()
    {
        // arrange
        var value = ZonedDateTimeExtensions.FromUnixTimeMilliseconds(60_000_000_000L);

        // assert
        value.Is(_moment);
    }

    [Fact]
    public void GetYearMonth()
    {
        // arrange
        var value = _moment.GetYearMonth();

        // assert
        value.Is(new YearMonth(_moment.Era, _moment.YearOfEra, _moment.Month, _moment.Calendar));
    }

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

    [Fact]
    public void ToUnixTimeMinutes()
    {
        // arrange
        var value = _moment.ToUnixTimeMinutes();

        // assert
        value.Is(1_000_000L);
    }

    [Fact]
    public void ToUnixTimeSeconds()
    {
        // arrange
        var value = _moment.ToUnixTimeSeconds();

        // assert
        value.Is(60_000_000L);
    }

    [Fact]
    public void ToUnixTimeMilliseconds()
    {
        // arrange
        var value = _moment.ToUnixTimeMilliseconds();

        // assert
        value.Is(60_000_000_000L);
    }

    [Fact]
    public void FloorToSecond()
    {
        Wrap(Period.FromMilliseconds(1999))
            .FloorToSecond()
            .Is(Wrap(Period.FromSeconds(1)));
    }

    [Fact]
    public void FloorToMinute()
    {
        Wrap(Period.FromSeconds(100))
            .FloorToMinute()
            .Is(Wrap(Period.FromMinutes(1)));
    }

    [Fact]
    public void FloorToHour()
    {
        Wrap(Period.FromMinutes(100))
            .FloorToHour()
            .Is(Wrap(Period.FromHours(1)));
    }

    [Fact]
    public void FloorToDay()
    {
        Wrap(Period.FromHours(30))
            .FloorToDay()
            .Is(Wrap(Period.FromDays(1)));
    }

    [Fact]
    public void FloorTo()
    {
        Wrap(Period.FromSeconds(55))
            .FloorTo(Period.FromSeconds(15))
            .Is(Wrap(Period.FromSeconds(45)));
    }

    [Fact]
    public void CeilToSecond()
    {
        Wrap(Period.FromMilliseconds(1))
            .CeilToSecond()
            .Is(Wrap(Period.FromSeconds(1)));
    }

    [Fact]
    public void CeilToMinute()
    {
        Wrap(Period.FromSeconds(1))
            .CeilToMinute()
            .Is(Wrap(Period.FromMinutes(1)));
    }

    [Fact]
    public void CeilToHour()
    {
        Wrap(Period.FromMinutes(1))
            .CeilToHour()
            .Is(Wrap(Period.FromHours(1)));
    }

    [Fact]
    public void CeilToDay()
    {
        Wrap(Period.FromHours(1))
            .CeilToDay()
            .Is(Wrap(Period.FromDays(1)));
    }

    [Fact]
    public void CeilTo()
    {
        Wrap(Period.FromSeconds(55))
            .CeilTo(Period.FromSeconds(15))
            .Is(Wrap(Period.FromSeconds(60)));
    }

    [Fact]
    public void RoundToSecond()
    {
        Wrap(Period.FromMilliseconds(499))
            .RoundToSecond()
            .Is(Wrap(Period.Zero));
        Wrap(Period.FromMilliseconds(500))
            .RoundToSecond()
            .Is(Wrap(Period.FromSeconds(1)));
    }

    [Fact]
    public void RoundToMinute()
    {
        Wrap(Period.FromSeconds(29))
            .RoundToMinute()
            .Is(Wrap(Period.Zero));
        Wrap(Period.FromSeconds(30))
            .RoundToMinute()
            .Is(Wrap(Period.FromMinutes(1)));
    }

    [Fact]
    public void RoundToHour()
    {
        Wrap(Period.FromMinutes(29))
            .RoundToHour()
            .Is(Wrap(Period.Zero));
        Wrap(Period.FromMinutes(30))
            .RoundToHour()
            .Is(Wrap(Period.FromHours(1)));
    }

    [Fact]
    public void RoundToDay()
    {
        Wrap(Period.FromHours(11))
            .RoundToDay()
            .Is(Wrap(Period.Zero));
        Wrap(Period.FromHours(12))
            .RoundToDay()
            .Is(Wrap(Period.FromDays(1)));
    }

    [Fact]
    public void RoundTo()
    {
        Wrap(Period.FromSeconds(50))
            .RoundTo(Period.FromSeconds(15))
            .Is(Wrap(Period.FromSeconds(45)));
        Wrap(Period.FromSeconds(55))
            .RoundTo(Period.FromSeconds(15))
            .Is(Wrap(Period.FromSeconds(60)));
    }

    private static ZonedDateTime Wrap(Period period) => new(Instant.MinValue + period.ToDuration(), DateTimeZone.Utc);
}