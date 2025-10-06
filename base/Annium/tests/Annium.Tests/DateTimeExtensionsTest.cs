using System;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for date time extension methods.
/// </summary>
public class DateTimeExtensionsTest
{
    /// <summary>
    /// The Unix epoch (January 1, 1970, 00:00:00 UTC).
    /// </summary>
    private static readonly DateTime _unixEpoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Verifies that FromUnixTimeMinutes converts Unix time in minutes to DateTime correctly.
    /// </summary>
    [Fact]
    public void FromUnixTimeMinutes_ReturnsCorrectDateTime()
    {
        // arrange
        var minutes = 60L;

        // act
        var result = DateTimeExtensions.FromUnixTimeMinutes(minutes);

        // assert
        result.Is(_unixEpoch.AddMinutes(60));
    }

    /// <summary>
    /// Verifies that FromUnixTimeSeconds converts Unix time in seconds to DateTime correctly.
    /// </summary>
    [Fact]
    public void FromUnixTimeSeconds_ReturnsCorrectDateTime()
    {
        // arrange
        var seconds = 3600L;

        // act
        var result = DateTimeExtensions.FromUnixTimeSeconds(seconds);

        // assert
        result.Is(_unixEpoch.AddSeconds(3600));
    }

    /// <summary>
    /// Verifies that FromUnixTimeMilliseconds converts Unix time in milliseconds to DateTime correctly.
    /// </summary>
    [Fact]
    public void FromUnixTimeMilliseconds_ReturnsCorrectDateTime()
    {
        // arrange
        var milliseconds = 1000L;

        // act
        var result = DateTimeExtensions.FromUnixTimeMilliseconds(milliseconds);

        // assert
        result.Is(_unixEpoch.AddMilliseconds(1000));
    }

    /// <summary>
    /// Verifies that FloorToSecond rounds a DateTime down to the nearest second.
    /// </summary>
    [Fact]
    public void FloorToSecond_RoundsDownToNearestSecond()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 30, 45, 678, DateTimeKind.Utc);

        // act
        var result = dt.FloorToSecond();

        // assert
        result.Is(new DateTime(2025, 1, 1, 12, 30, 45, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that FloorToMinute rounds a DateTime down to the nearest minute.
    /// </summary>
    [Fact]
    public void FloorToMinute_RoundsDownToNearestMinute()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 30, 45, 678, DateTimeKind.Utc);

        // act
        var result = dt.FloorToMinute();

        // assert
        result.Is(new DateTime(2025, 1, 1, 12, 30, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that FloorToHour rounds a DateTime down to the nearest hour.
    /// </summary>
    [Fact]
    public void FloorToHour_RoundsDownToNearestHour()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 30, 45, 678, DateTimeKind.Utc);

        // act
        var result = dt.FloorToHour();

        // assert
        result.Is(new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that FloorToDay rounds a DateTime down to the nearest day.
    /// </summary>
    [Fact]
    public void FloorToDay_RoundsDownToNearestDay()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 30, 45, 678, DateTimeKind.Utc);

        // act
        var result = dt.FloorToDay();

        // assert
        result.Is(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that FloorTo rounds a DateTime down to the nearest multiple of a specified TimeSpan.
    /// </summary>
    [Fact]
    public void FloorTo_RoundsDownToSpecifiedTimeSpan()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 37, 45, 0, DateTimeKind.Utc);
        var interval = TimeSpan.FromMinutes(15);

        // act
        var result = dt.FloorTo(interval);

        // assert
        result.Is(new DateTime(2025, 1, 1, 12, 30, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that RoundToSecond rounds a DateTime to the nearest second.
    /// </summary>
    [Fact]
    public void RoundToSecond_RoundsToNearestSecond()
    {
        // arrange
        var dt1 = new DateTime(2025, 1, 1, 12, 30, 45, 400, DateTimeKind.Utc);
        var dt2 = new DateTime(2025, 1, 1, 12, 30, 45, 600, DateTimeKind.Utc);

        // act
        var result1 = dt1.RoundToSecond();
        var result2 = dt2.RoundToSecond();

        // assert
        result1.Is(new DateTime(2025, 1, 1, 12, 30, 45, 0, DateTimeKind.Utc));
        result2.Is(new DateTime(2025, 1, 1, 12, 30, 46, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that RoundToMinute rounds a DateTime to the nearest minute.
    /// </summary>
    [Fact]
    public void RoundToMinute_RoundsToNearestMinute()
    {
        // arrange
        var dt1 = new DateTime(2025, 1, 1, 12, 30, 20, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(2025, 1, 1, 12, 30, 40, 0, DateTimeKind.Utc);

        // act
        var result1 = dt1.RoundToMinute();
        var result2 = dt2.RoundToMinute();

        // assert
        result1.Is(new DateTime(2025, 1, 1, 12, 30, 0, 0, DateTimeKind.Utc));
        result2.Is(new DateTime(2025, 1, 1, 12, 31, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that RoundToHour rounds a DateTime to the nearest hour.
    /// </summary>
    [Fact]
    public void RoundToHour_RoundsToNearestHour()
    {
        // arrange
        var dt1 = new DateTime(2025, 1, 1, 12, 20, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(2025, 1, 1, 12, 40, 0, 0, DateTimeKind.Utc);

        // act
        var result1 = dt1.RoundToHour();
        var result2 = dt2.RoundToHour();

        // assert
        result1.Is(new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc));
        result2.Is(new DateTime(2025, 1, 1, 13, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that RoundToDay rounds a DateTime to the nearest day.
    /// </summary>
    [Fact]
    public void RoundToDay_RoundsToNearestDay()
    {
        // arrange
        var dt1 = new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(2025, 1, 1, 14, 0, 0, 0, DateTimeKind.Utc);

        // act
        var result1 = dt1.RoundToDay();
        var result2 = dt2.RoundToDay();

        // assert
        result1.Is(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        result2.Is(new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that RoundTo rounds a DateTime to the nearest multiple of a specified TimeSpan.
    /// </summary>
    [Fact]
    public void RoundTo_RoundsToSpecifiedTimeSpan()
    {
        // arrange
        var dt1 = new DateTime(2025, 1, 1, 12, 34, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(2025, 1, 1, 12, 38, 0, 0, DateTimeKind.Utc);
        var interval = TimeSpan.FromMinutes(15);

        // act
        var result1 = dt1.RoundTo(interval);
        var result2 = dt2.RoundTo(interval);

        // assert
        result1.Is(new DateTime(2025, 1, 1, 12, 30, 0, 0, DateTimeKind.Utc));
        result2.Is(new DateTime(2025, 1, 1, 12, 45, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that CeilToSecond rounds a DateTime up to the nearest second.
    /// </summary>
    [Fact]
    public void CeilToSecond_RoundsUpToNearestSecond()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 30, 45, 100, DateTimeKind.Utc);

        // act
        var result = dt.CeilToSecond();

        // assert
        result.Is(new DateTime(2025, 1, 1, 12, 30, 46, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that CeilToMinute rounds a DateTime up to the nearest minute.
    /// </summary>
    [Fact]
    public void CeilToMinute_RoundsUpToNearestMinute()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 30, 1, 0, DateTimeKind.Utc);

        // act
        var result = dt.CeilToMinute();

        // assert
        result.Is(new DateTime(2025, 1, 1, 12, 31, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that CeilToHour rounds a DateTime up to the nearest hour.
    /// </summary>
    [Fact]
    public void CeilToHour_RoundsUpToNearestHour()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 1, 0, 0, DateTimeKind.Utc);

        // act
        var result = dt.CeilToHour();

        // assert
        result.Is(new DateTime(2025, 1, 1, 13, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that CeilToDay rounds a DateTime up to the nearest day.
    /// </summary>
    [Fact]
    public void CeilToDay_RoundsUpToNearestDay()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 0, 0, 1, 0, DateTimeKind.Utc);

        // act
        var result = dt.CeilToDay();

        // assert
        result.Is(new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that InUtc returns UTC DateTime unchanged.
    /// </summary>
    [Fact]
    public void InUtc_UtcKind_ReturnsUnchanged()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 30, 45, 0, DateTimeKind.Utc);

        // act
        var result = dt.InUtc();

        // assert
        result.Kind.Is(DateTimeKind.Utc);
        result.Is(dt);
    }

    /// <summary>
    /// Verifies that InUtc converts Local DateTime to UTC with timezone offset.
    /// </summary>
    [Fact]
    public void InUtc_LocalKind_ConvertsWithOffset()
    {
        // arrange
        var localDt = new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Local);
        var offset = TimeZoneInfo.Local.GetUtcOffset(localDt);

        // act
        var result = localDt.InUtc();

        // assert
        result.Kind.Is(DateTimeKind.Utc);
        result.Is(localDt - offset);
    }

    /// <summary>
    /// Verifies that InUtc converts Unspecified DateTime to UTC with timezone offset.
    /// </summary>
    [Fact]
    public void InUtc_UnspecifiedKind_ConvertsWithOffset()
    {
        // arrange
        var dt = new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Unspecified);
        var offset = TimeZoneInfo.Local.GetUtcOffset(dt);

        // act
        var result = dt.InUtc();

        // assert
        result.Kind.Is(DateTimeKind.Utc);
        result.Is(dt - offset);
    }

    /// <summary>
    /// Verifies that ToUnixTimeMinutes converts DateTime to Unix time in minutes correctly.
    /// </summary>
    [Fact]
    public void ToUnixTimeMinutes_ConvertsCorrectly()
    {
        // arrange
        var dt = _unixEpoch.AddMinutes(120);

        // act
        var result = dt.ToUnixTimeMinutes();

        // assert
        result.Is(120L);
    }

    /// <summary>
    /// Verifies that ToUnixTimeSeconds converts DateTime to Unix time in seconds correctly.
    /// </summary>
    [Fact]
    public void ToUnixTimeSeconds_ConvertsCorrectly()
    {
        // arrange
        var dt = _unixEpoch.AddSeconds(7200);

        // act
        var result = dt.ToUnixTimeSeconds();

        // assert
        result.Is(7200L);
    }

    /// <summary>
    /// Verifies that ToUnixTimeMilliseconds converts DateTime to Unix time in milliseconds correctly.
    /// </summary>
    [Fact]
    public void ToUnixTimeMilliseconds_ConvertsCorrectly()
    {
        // arrange
        var dt = _unixEpoch.AddMilliseconds(5000);

        // act
        var result = dt.ToUnixTimeMilliseconds();

        // assert
        result.Is(5000L);
    }

    /// <summary>
    /// Verifies that ToUnixTimeMinutes floors fractional minutes.
    /// </summary>
    [Fact]
    public void ToUnixTimeMinutes_FloorsFractionalMinutes()
    {
        // arrange
        var dt = _unixEpoch.AddMinutes(5).AddSeconds(30);

        // act
        var result = dt.ToUnixTimeMinutes();

        // assert
        result.Is(5L);
    }

    /// <summary>
    /// Verifies that ToUnixTimeSeconds floors fractional seconds.
    /// </summary>
    [Fact]
    public void ToUnixTimeSeconds_FloorsFractionalSeconds()
    {
        // arrange
        var dt = _unixEpoch.AddSeconds(10).AddMilliseconds(500);

        // act
        var result = dt.ToUnixTimeSeconds();

        // assert
        result.Is(10L);
    }

    /// <summary>
    /// Verifies that ToUnixTimeMilliseconds floors fractional milliseconds.
    /// </summary>
    [Fact]
    public void ToUnixTimeMilliseconds_FloorsFractionalMilliseconds()
    {
        // arrange
        var dt = _unixEpoch.AddMilliseconds(100.7);

        // act
        var result = dt.ToUnixTimeMilliseconds();

        // assert
        result.Is(100L);
    }
}
