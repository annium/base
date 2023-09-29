using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class DurationExtensionsTest
{
    [Fact]
    public void FloorToSecond()
    {
        Duration.FromMilliseconds(1999)
            .FloorToSecond()
            .Is(Duration.FromSeconds(1));
    }

    [Fact]
    public void FloorToMinute()
    {
        Duration.FromSeconds(100)
            .FloorToMinute()
            .Is(Duration.FromMinutes(1));
    }

    [Fact]
    public void FloorToHour()
    {
        Duration.FromMinutes(100)
            .FloorToHour()
            .Is(Duration.FromHours(1));
    }

    [Fact]
    public void FloorToDay()
    {
        Duration.FromHours(30)
            .FloorToDay()
            .Is(Duration.FromDays(1));
    }

    [Fact]
    public void FloorTo()
    {
        Duration.FromSeconds(55)
            .FloorTo(Duration.FromSeconds(15))
            .Is(Duration.FromSeconds(45));
    }

    [Fact]
    public void CeilToSecond()
    {
        Duration.FromMilliseconds(1)
            .CeilToSecond()
            .Is(Duration.FromSeconds(1));
    }

    [Fact]
    public void CeilToMinute()
    {
        Duration.FromSeconds(1)
            .CeilToMinute()
            .Is(Duration.FromMinutes(1));
    }

    [Fact]
    public void CeilToHour()
    {
        Duration.FromMinutes(1)
            .CeilToHour()
            .Is(Duration.FromHours(1));
    }

    [Fact]
    public void CeilToDay()
    {
        Duration.FromHours(1)
            .CeilToDay()
            .Is(Duration.FromDays(1));
    }

    [Fact]
    public void CeilTo()
    {
        Duration.FromSeconds(55)
            .CeilTo(Duration.FromSeconds(15))
            .Is(Duration.FromSeconds(60));
    }

    [Fact]
    public void RoundToSecond()
    {
        Duration.FromMilliseconds(499)
            .RoundToSecond()
            .Is(Duration.Zero);
        Duration.FromMilliseconds(500)
            .RoundToSecond()
            .Is(Duration.FromSeconds(1));
    }

    [Fact]
    public void RoundToMinute()
    {
        Duration.FromSeconds(29)
            .RoundToMinute()
            .Is(Duration.Zero);
        Duration.FromSeconds(30)
            .RoundToMinute()
            .Is(Duration.FromMinutes(1));
    }

    [Fact]
    public void RoundToHour()
    {
        Duration.FromMinutes(29)
            .RoundToHour()
            .Is(Duration.Zero);
        Duration.FromMinutes(30)
            .RoundToHour()
            .Is(Duration.FromHours(1));
    }

    [Fact]
    public void RoundToDay()
    {
        Duration.FromHours(11)
            .RoundToDay()
            .Is(Duration.Zero);
        Duration.FromHours(12)
            .RoundToDay()
            .Is(Duration.FromDays(1));
    }

    [Fact]
    public void RoundTo()
    {
        Duration.FromSeconds(50)
            .RoundTo(Duration.FromSeconds(15))
            .Is(Duration.FromSeconds(45));
        Duration.FromSeconds(55)
            .RoundTo(Duration.FromSeconds(15))
            .Is(Duration.FromSeconds(60));
    }
}