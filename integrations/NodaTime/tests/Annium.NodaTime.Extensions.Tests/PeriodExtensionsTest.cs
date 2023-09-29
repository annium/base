using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class PeriodExtensionsTest
{
    [Fact]
    public void FloorToSecond()
    {
        Period.FromMilliseconds(1999)
            .FloorToSecond()
            .Normalize()
            .Is(Period.FromSeconds(1).Normalize());
    }

    [Fact]
    public void FloorToMinute()
    {
        Period.FromSeconds(100)
            .FloorToMinute()
            .Normalize()
            .Is(Period.FromMinutes(1).Normalize());
    }

    [Fact]
    public void FloorToHour()
    {
        Period.FromMinutes(100)
            .FloorToHour()
            .Normalize()
            .Is(Period.FromHours(1).Normalize());
    }

    [Fact]
    public void FloorToDay()
    {
        Period.FromHours(30)
            .FloorToDay()
            .Normalize()
            .Is(Period.FromDays(1).Normalize());
    }

    [Fact]
    public void FloorTo()
    {
        Period.FromSeconds(55)
            .FloorTo(Period.FromSeconds(15))
            .Normalize()
            .Is(Period.FromSeconds(45).Normalize());
    }

    [Fact]
    public void CeilToSecond()
    {
        Period.FromMilliseconds(1)
            .CeilToSecond()
            .Normalize()
            .Is(Period.FromSeconds(1).Normalize());
    }

    [Fact]
    public void CeilToMinute()
    {
        Period.FromSeconds(1)
            .CeilToMinute()
            .Normalize()
            .Is(Period.FromMinutes(1).Normalize());
    }

    [Fact]
    public void CeilToHour()
    {
        Period.FromMinutes(1)
            .CeilToHour()
            .Normalize()
            .Is(Period.FromHours(1).Normalize());
    }

    [Fact]
    public void CeilToDay()
    {
        Period.FromHours(1)
            .CeilToDay()
            .Normalize()
            .Is(Period.FromDays(1).Normalize());
    }

    [Fact]
    public void CeilTo()
    {
        Period.FromSeconds(55)
            .CeilTo(Period.FromSeconds(15))
            .Normalize()
            .Is(Period.FromSeconds(60).Normalize());
    }

    [Fact]
    public void RoundToSecond()
    {
        Period.FromMilliseconds(499)
            .RoundToSecond()
            .Normalize()
            .Is(Period.Zero.Normalize());
        Period.FromMilliseconds(500)
            .RoundToSecond()
            .Normalize()
            .Is(Period.FromSeconds(1));
    }

    [Fact]
    public void RoundToMinute()
    {
        Period.FromSeconds(29)
            .RoundToMinute()
            .Normalize()
            .Is(Period.Zero.Normalize());
        Period.FromSeconds(30)
            .RoundToMinute()
            .Normalize()
            .Is(Period.FromMinutes(1).Normalize());
    }

    [Fact]
    public void RoundToHour()
    {
        Period.FromMinutes(29)
            .RoundToHour()
            .Normalize()
            .Is(Period.Zero.Normalize());
        Period.FromMinutes(30)
            .RoundToHour()
            .Normalize()
            .Is(Period.FromHours(1).Normalize());
    }

    [Fact]
    public void RoundToDay()
    {
        Period.FromHours(11)
            .RoundToDay()
            .Normalize()
            .Is(Period.Zero.Normalize());
        Period.FromHours(12)
            .RoundToDay()
            .Normalize()
            .Is(Period.FromDays(1).Normalize());
    }

    [Fact]
    public void RoundTo()
    {
        Period.FromSeconds(50)
            .RoundTo(Period.FromSeconds(15))
            .Normalize()
            .Is(Period.FromSeconds(45).Normalize());
        Period.FromSeconds(55)
            .RoundTo(Period.FromSeconds(15))
            .Normalize()
            .Is(Period.FromSeconds(60).Normalize());
    }
}