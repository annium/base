using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

/// <summary>
/// Tests for period extension methods that provide convenient floor, ceiling, and rounding operations for time periods.
/// </summary>
public class PeriodExtensionsTest
{
    /// <summary>
    /// Tests that FloorToSecond floors a period to the nearest second boundary below.
    /// </summary>
    [Fact]
    public void FloorToSecond()
    {
        Period.FromMilliseconds(1999).FloorToSecond().Normalize().Is(Period.FromSeconds(1).Normalize());
    }

    /// <summary>
    /// Tests that FloorToMinute floors a period to the nearest minute boundary below.
    /// </summary>
    [Fact]
    public void FloorToMinute()
    {
        Period.FromSeconds(100).FloorToMinute().Normalize().Is(Period.FromMinutes(1).Normalize());
    }

    /// <summary>
    /// Tests that FloorToHour floors a period to the nearest hour boundary below.
    /// </summary>
    [Fact]
    public void FloorToHour()
    {
        Period.FromMinutes(100).FloorToHour().Normalize().Is(Period.FromHours(1).Normalize());
    }

    /// <summary>
    /// Tests that FloorToDay floors a period to the nearest day boundary below.
    /// </summary>
    [Fact]
    public void FloorToDay()
    {
        Period.FromHours(30).FloorToDay().Normalize().Is(Period.FromDays(1).Normalize());
    }

    /// <summary>
    /// Tests that FloorTo floors a period to the nearest boundary of a specified period below.
    /// </summary>
    [Fact]
    public void FloorTo()
    {
        Period.FromSeconds(55).FloorTo(Period.FromSeconds(15)).Normalize().Is(Period.FromSeconds(45).Normalize());
    }

    /// <summary>
    /// Tests that CeilToSecond ceils a period to the nearest second boundary above.
    /// </summary>
    [Fact]
    public void CeilToSecond()
    {
        Period.FromMilliseconds(1).CeilToSecond().Normalize().Is(Period.FromSeconds(1).Normalize());
    }

    /// <summary>
    /// Tests that CeilToMinute ceils a period to the nearest minute boundary above.
    /// </summary>
    [Fact]
    public void CeilToMinute()
    {
        Period.FromSeconds(1).CeilToMinute().Normalize().Is(Period.FromMinutes(1).Normalize());
    }

    /// <summary>
    /// Tests that CeilToHour ceils a period to the nearest hour boundary above.
    /// </summary>
    [Fact]
    public void CeilToHour()
    {
        Period.FromMinutes(1).CeilToHour().Normalize().Is(Period.FromHours(1).Normalize());
    }

    /// <summary>
    /// Tests that CeilToDay ceils a period to the nearest day boundary above.
    /// </summary>
    [Fact]
    public void CeilToDay()
    {
        Period.FromHours(1).CeilToDay().Normalize().Is(Period.FromDays(1).Normalize());
    }

    /// <summary>
    /// Tests that CeilTo ceils a period to the nearest boundary of a specified period above.
    /// </summary>
    [Fact]
    public void CeilTo()
    {
        Period.FromSeconds(55).CeilTo(Period.FromSeconds(15)).Normalize().Is(Period.FromSeconds(60).Normalize());
    }

    /// <summary>
    /// Tests that RoundToSecond rounds a period to the nearest second boundary.
    /// </summary>
    [Fact]
    public void RoundToSecond()
    {
        Period.FromMilliseconds(499).RoundToSecond().Normalize().Is(Period.Zero.Normalize());
        Period.FromMilliseconds(500).RoundToSecond().Normalize().Is(Period.FromSeconds(1));
    }

    /// <summary>
    /// Tests that RoundToMinute rounds a period to the nearest minute boundary.
    /// </summary>
    [Fact]
    public void RoundToMinute()
    {
        Period.FromSeconds(29).RoundToMinute().Normalize().Is(Period.Zero.Normalize());
        Period.FromSeconds(30).RoundToMinute().Normalize().Is(Period.FromMinutes(1).Normalize());
    }

    /// <summary>
    /// Tests that RoundToHour rounds a period to the nearest hour boundary.
    /// </summary>
    [Fact]
    public void RoundToHour()
    {
        Period.FromMinutes(29).RoundToHour().Normalize().Is(Period.Zero.Normalize());
        Period.FromMinutes(30).RoundToHour().Normalize().Is(Period.FromHours(1).Normalize());
    }

    /// <summary>
    /// Tests that RoundToDay rounds a period to the nearest day boundary.
    /// </summary>
    [Fact]
    public void RoundToDay()
    {
        Period.FromHours(11).RoundToDay().Normalize().Is(Period.Zero.Normalize());
        Period.FromHours(12).RoundToDay().Normalize().Is(Period.FromDays(1).Normalize());
    }

    /// <summary>
    /// Tests that RoundTo rounds a period to the nearest boundary of a specified period.
    /// </summary>
    [Fact]
    public void RoundTo()
    {
        Period.FromSeconds(50).RoundTo(Period.FromSeconds(15)).Normalize().Is(Period.FromSeconds(45).Normalize());
        Period.FromSeconds(55).RoundTo(Period.FromSeconds(15)).Normalize().Is(Period.FromSeconds(60).Normalize());
    }
}
