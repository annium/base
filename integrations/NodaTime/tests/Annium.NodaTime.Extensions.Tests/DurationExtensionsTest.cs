using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

/// <summary>
/// Tests for duration extension methods that provide convenient floor, ceiling, and rounding operations.
/// </summary>
public class DurationExtensionsTest
{
    /// <summary>
    /// Tests that FloorToSecond floors a duration to the nearest second boundary below.
    /// </summary>
    [Fact]
    public void FloorToSecond()
    {
        Duration.FromMilliseconds(1999).FloorToSecond().Is(Duration.FromSeconds(1));
    }

    /// <summary>
    /// Tests that FloorToMinute floors a duration to the nearest minute boundary below.
    /// </summary>
    [Fact]
    public void FloorToMinute()
    {
        Duration.FromSeconds(100).FloorToMinute().Is(Duration.FromMinutes(1));
    }

    /// <summary>
    /// Tests that FloorToHour floors a duration to the nearest hour boundary below.
    /// </summary>
    [Fact]
    public void FloorToHour()
    {
        Duration.FromMinutes(100).FloorToHour().Is(Duration.FromHours(1));
    }

    /// <summary>
    /// Tests that FloorToDay floors a duration to the nearest day boundary below.
    /// </summary>
    [Fact]
    public void FloorToDay()
    {
        Duration.FromHours(30).FloorToDay().Is(Duration.FromDays(1));
    }

    /// <summary>
    /// Tests that FloorTo floors a duration to the nearest boundary of a specified duration below.
    /// </summary>
    [Fact]
    public void FloorTo()
    {
        Duration.FromSeconds(55).FloorTo(Duration.FromSeconds(15)).Is(Duration.FromSeconds(45));
    }

    /// <summary>
    /// Tests that CeilToSecond ceils a duration to the nearest second boundary above.
    /// </summary>
    [Fact]
    public void CeilToSecond()
    {
        Duration.FromMilliseconds(1).CeilToSecond().Is(Duration.FromSeconds(1));
    }

    /// <summary>
    /// Tests that CeilToMinute ceils a duration to the nearest minute boundary above.
    /// </summary>
    [Fact]
    public void CeilToMinute()
    {
        Duration.FromSeconds(1).CeilToMinute().Is(Duration.FromMinutes(1));
    }

    /// <summary>
    /// Tests that CeilToHour ceils a duration to the nearest hour boundary above.
    /// </summary>
    [Fact]
    public void CeilToHour()
    {
        Duration.FromMinutes(1).CeilToHour().Is(Duration.FromHours(1));
    }

    /// <summary>
    /// Tests that CeilToDay ceils a duration to the nearest day boundary above.
    /// </summary>
    [Fact]
    public void CeilToDay()
    {
        Duration.FromHours(1).CeilToDay().Is(Duration.FromDays(1));
    }

    /// <summary>
    /// Tests that CeilTo ceils a duration to the nearest boundary of a specified duration above.
    /// </summary>
    [Fact]
    public void CeilTo()
    {
        Duration.FromSeconds(55).CeilTo(Duration.FromSeconds(15)).Is(Duration.FromSeconds(60));
    }

    /// <summary>
    /// Tests that RoundToSecond rounds a duration to the nearest second boundary.
    /// </summary>
    [Fact]
    public void RoundToSecond()
    {
        Duration.FromMilliseconds(499).RoundToSecond().Is(Duration.Zero);
        Duration.FromMilliseconds(500).RoundToSecond().Is(Duration.FromSeconds(1));
    }

    /// <summary>
    /// Tests that RoundToMinute rounds a duration to the nearest minute boundary.
    /// </summary>
    [Fact]
    public void RoundToMinute()
    {
        Duration.FromSeconds(29).RoundToMinute().Is(Duration.Zero);
        Duration.FromSeconds(30).RoundToMinute().Is(Duration.FromMinutes(1));
    }

    /// <summary>
    /// Tests that RoundToHour rounds a duration to the nearest hour boundary.
    /// </summary>
    [Fact]
    public void RoundToHour()
    {
        Duration.FromMinutes(29).RoundToHour().Is(Duration.Zero);
        Duration.FromMinutes(30).RoundToHour().Is(Duration.FromHours(1));
    }

    /// <summary>
    /// Tests that RoundToDay rounds a duration to the nearest day boundary.
    /// </summary>
    [Fact]
    public void RoundToDay()
    {
        Duration.FromHours(11).RoundToDay().Is(Duration.Zero);
        Duration.FromHours(12).RoundToDay().Is(Duration.FromDays(1));
    }

    /// <summary>
    /// Tests that RoundTo rounds a duration to the nearest boundary of a specified duration.
    /// </summary>
    [Fact]
    public void RoundTo()
    {
        Duration.FromSeconds(50).RoundTo(Duration.FromSeconds(15)).Is(Duration.FromSeconds(45));
        Duration.FromSeconds(55).RoundTo(Duration.FromSeconds(15)).Is(Duration.FromSeconds(60));
    }
}
