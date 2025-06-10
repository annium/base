using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

/// <summary>
/// Tests for instant extension methods that provide Unix time conversion, date/time checks, and duration operations.
/// </summary>
public class InstantExtensionsTest
{
    /// <summary>
    /// Tests that FromUnixTimeMinutes correctly converts Unix time in minutes to an Instant.
    /// </summary>
    [Fact]
    public void FromUnixTimeMinutes()
    {
        // arrange
        var value = InstantExtensions.FromUnixTimeMinutes(1_000_000L);

        // assert
        value.Is(new LocalDateTime(1971, 11, 26, 10, 40).InUtc().ToInstant());
    }

    /// <summary>
    /// Tests that IsMidnight correctly identifies instants that occur at midnight UTC.
    /// </summary>
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

    /// <summary>
    /// Tests that ToUnixTimeMinutes correctly converts an Instant to Unix time in minutes.
    /// </summary>
    [Fact]
    public void ToUnixTimeMinutes()
    {
        // arrange
        var value = new LocalDateTime(1971, 11, 26, 10, 40).InUtc().ToInstant().ToUnixTimeMinutes();

        // assert
        value.Is(1_000_000L);
    }

    /// <summary>
    /// Tests that FloorToSecond floors an instant's duration to the nearest second boundary below.
    /// </summary>
    [Fact]
    public void FloorToSecond()
    {
        Wrap(Duration.FromMilliseconds(1999)).FloorToSecond().Is(Wrap(Duration.FromSeconds(1)));
    }

    /// <summary>
    /// Tests that FloorToMinute floors an instant's duration to the nearest minute boundary below.
    /// </summary>
    [Fact]
    public void FloorToMinute()
    {
        Wrap(Duration.FromSeconds(100)).FloorToMinute().Is(Wrap(Duration.FromMinutes(1)));
    }

    /// <summary>
    /// Tests that FloorToHour floors an instant's duration to the nearest hour boundary below.
    /// </summary>
    [Fact]
    public void FloorToHour()
    {
        Wrap(Duration.FromMinutes(100)).FloorToHour().Is(Wrap(Duration.FromHours(1)));
    }

    /// <summary>
    /// Tests that FloorToDay floors an instant's duration to the nearest day boundary below.
    /// </summary>
    [Fact]
    public void FloorToDay()
    {
        Wrap(Duration.FromHours(30)).FloorToDay().Is(Wrap(Duration.FromDays(1)));
    }

    /// <summary>
    /// Tests that FloorTo floors an instant's duration to the nearest boundary of a specified duration below.
    /// </summary>
    [Fact]
    public void FloorTo()
    {
        Wrap(Duration.FromSeconds(55)).FloorTo(Duration.FromSeconds(15)).Is(Wrap(Duration.FromSeconds(45)));
    }

    /// <summary>
    /// Tests that CeilToSecond ceils an instant's duration to the nearest second boundary above.
    /// </summary>
    [Fact]
    public void CeilToSecond()
    {
        Wrap(Duration.FromMilliseconds(1)).CeilToSecond().Is(Wrap(Duration.FromSeconds(1)));
    }

    /// <summary>
    /// Tests that CeilToMinute ceils an instant's duration to the nearest minute boundary above.
    /// </summary>
    [Fact]
    public void CeilToMinute()
    {
        Wrap(Duration.FromSeconds(1)).CeilToMinute().Is(Wrap(Duration.FromMinutes(1)));
    }

    /// <summary>
    /// Tests that CeilToHour ceils an instant's duration to the nearest hour boundary above.
    /// </summary>
    [Fact]
    public void CeilToHour()
    {
        Wrap(Duration.FromMinutes(1)).CeilToHour().Is(Wrap(Duration.FromHours(1)));
    }

    /// <summary>
    /// Tests that CeilToDay ceils an instant's duration to the nearest day boundary above.
    /// </summary>
    [Fact]
    public void CeilToDay()
    {
        Wrap(Duration.FromHours(1)).CeilToDay().Is(Wrap(Duration.FromDays(1)));
    }

    /// <summary>
    /// Tests that CeilTo ceils an instant's duration to the nearest boundary of a specified duration above.
    /// </summary>
    [Fact]
    public void CeilTo()
    {
        Wrap(Duration.FromSeconds(55)).CeilTo(Duration.FromSeconds(15)).Is(Wrap(Duration.FromSeconds(60)));
    }

    /// <summary>
    /// Tests that RoundToSecond rounds an instant's duration to the nearest second boundary.
    /// </summary>
    [Fact]
    public void RoundToSecond()
    {
        Wrap(Duration.FromMilliseconds(499)).RoundToSecond().Is(Wrap(Duration.Zero));
        Wrap(Duration.FromMilliseconds(500)).RoundToSecond().Is(Wrap(Duration.FromSeconds(1)));
    }

    /// <summary>
    /// Tests that RoundToMinute rounds an instant's duration to the nearest minute boundary.
    /// </summary>
    [Fact]
    public void RoundToMinute()
    {
        Wrap(Duration.FromSeconds(29)).RoundToMinute().Is(Wrap(Duration.Zero));
        Wrap(Duration.FromSeconds(30)).RoundToMinute().Is(Wrap(Duration.FromMinutes(1)));
    }

    /// <summary>
    /// Tests that RoundToHour rounds an instant's duration to the nearest hour boundary.
    /// </summary>
    [Fact]
    public void RoundToHour()
    {
        Wrap(Duration.FromMinutes(29)).RoundToHour().Is(Wrap(Duration.Zero));
        Wrap(Duration.FromMinutes(30)).RoundToHour().Is(Wrap(Duration.FromHours(1)));
    }

    /// <summary>
    /// Tests that RoundToDay rounds an instant's duration to the nearest day boundary.
    /// </summary>
    [Fact]
    public void RoundToDay()
    {
        Wrap(Duration.FromHours(11)).RoundToDay().Is(Wrap(Duration.Zero));
        Wrap(Duration.FromHours(12)).RoundToDay().Is(Wrap(Duration.FromDays(1)));
    }

    /// <summary>
    /// Tests that RoundTo rounds an instant's duration to the nearest boundary of a specified duration.
    /// </summary>
    [Fact]
    public void RoundTo()
    {
        Wrap(Duration.FromSeconds(50)).RoundTo(Duration.FromSeconds(15)).Is(Wrap(Duration.FromSeconds(45)));
        Wrap(Duration.FromSeconds(55)).RoundTo(Duration.FromSeconds(15)).Is(Wrap(Duration.FromSeconds(60)));
    }

    /// <summary>
    /// Helper method to convert a Duration to an Instant by adding it to Instant.MinValue.
    /// </summary>
    /// <param name="duration">The duration to convert.</param>
    /// <returns>An Instant representing the MinValue plus the given duration.</returns>
    private static Instant Wrap(Duration duration) => Instant.MinValue + duration;
}
