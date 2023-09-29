using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class InstantExtensionsTest
{
    [Fact]
    public void FromUnixTimeMinutes()
    {
        // arrange
        var value = InstantExtensions.FromUnixTimeMinutes(1_000_000L);

        // assert
        value.Is(new LocalDateTime(1971, 11, 26, 10, 40).InUtc().ToInstant());
    }

    [Fact]
    public void IsMidnight()
    {
        // arrange
        var mignight = new LocalDateTime(1, 2, 3, 0, 0, 0, 0).InUtc().ToInstant();
        var nonMignight = new LocalDateTime(1, 2, 3, 0, 0, 0, 1).InUtc().ToInstant();

        // assert
        mignight.IsMidnight().IsTrue();
        nonMignight.IsMidnight().IsFalse();
    }

    [Fact]
    public void ToUnixTimeMinutes()
    {
        // arrange
        var value = new LocalDateTime(1971, 11, 26, 10, 40).InUtc().ToInstant().ToUnixTimeMinutes();

        // assert
        value.Is(1_000_000L);
    }

    [Fact]
    public void FloorToSecond()
    {
        Wrap(Duration.FromMilliseconds(1999))
            .FloorToSecond()
            .Is(Wrap(Duration.FromSeconds(1)));
    }

    [Fact]
    public void FloorToMinute()
    {
        Wrap(Duration.FromSeconds(100))
            .FloorToMinute()
            .Is(Wrap(Duration.FromMinutes(1)));
    }

    [Fact]
    public void FloorToHour()
    {
        Wrap(Duration.FromMinutes(100))
            .FloorToHour()
            .Is(Wrap(Duration.FromHours(1)));
    }

    [Fact]
    public void FloorToDay()
    {
        Wrap(Duration.FromHours(30))
            .FloorToDay()
            .Is(Wrap(Duration.FromDays(1)));
    }

    [Fact]
    public void FloorTo()
    {
        Wrap(Duration.FromSeconds(55))
            .FloorTo(Duration.FromSeconds(15))
            .Is(Wrap(Duration.FromSeconds(45)));
    }

    [Fact]
    public void CeilToSecond()
    {
        Wrap(Duration.FromMilliseconds(1))
            .CeilToSecond()
            .Is(Wrap(Duration.FromSeconds(1)));
    }

    [Fact]
    public void CeilToMinute()
    {
        Wrap(Duration.FromSeconds(1))
            .CeilToMinute()
            .Is(Wrap(Duration.FromMinutes(1)));
    }

    [Fact]
    public void CeilToHour()
    {
        Wrap(Duration.FromMinutes(1))
            .CeilToHour()
            .Is(Wrap(Duration.FromHours(1)));
    }

    [Fact]
    public void CeilToDay()
    {
        Wrap(Duration.FromHours(1))
            .CeilToDay()
            .Is(Wrap(Duration.FromDays(1)));
    }

    [Fact]
    public void CeilTo()
    {
        Wrap(Duration.FromSeconds(55))
            .CeilTo(Duration.FromSeconds(15))
            .Is(Wrap(Duration.FromSeconds(60)));
    }

    [Fact]
    public void RoundToSecond()
    {
        Wrap(Duration.FromMilliseconds(499))
            .RoundToSecond()
            .Is(Wrap(Duration.Zero));
        Wrap(Duration.FromMilliseconds(500))
            .RoundToSecond()
            .Is(Wrap(Duration.FromSeconds(1)));
    }

    [Fact]
    public void RoundToMinute()
    {
        Wrap(Duration.FromSeconds(29))
            .RoundToMinute()
            .Is(Wrap(Duration.Zero));
        Wrap(Duration.FromSeconds(30))
            .RoundToMinute()
            .Is(Wrap(Duration.FromMinutes(1)));
    }

    [Fact]
    public void RoundToHour()
    {
        Wrap(Duration.FromMinutes(29))
            .RoundToHour()
            .Is(Wrap(Duration.Zero));
        Wrap(Duration.FromMinutes(30))
            .RoundToHour()
            .Is(Wrap(Duration.FromHours(1)));
    }

    [Fact]
    public void RoundToDay()
    {
        Wrap(Duration.FromHours(11))
            .RoundToDay()
            .Is(Wrap(Duration.Zero));
        Wrap(Duration.FromHours(12))
            .RoundToDay()
            .Is(Wrap(Duration.FromDays(1)));
    }

    [Fact]
    public void RoundTo()
    {
        Wrap(Duration.FromSeconds(50))
            .RoundTo(Duration.FromSeconds(15))
            .Is(Wrap(Duration.FromSeconds(45)));
        Wrap(Duration.FromSeconds(55))
            .RoundTo(Duration.FromSeconds(15))
            .Is(Wrap(Duration.FromSeconds(60)));
    }

    private static Instant Wrap(Duration duration) => Instant.MinValue + duration;
}