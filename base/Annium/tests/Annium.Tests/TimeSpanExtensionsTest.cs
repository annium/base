using System;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Tests for <see cref="TimeSpanExtensions"/> — especially the negative-input cases that are the
/// durable bug-class home for the signed-modulo cluster (B10-B12 in review annium-2).
/// </summary>
public class TimeSpanExtensionsTest
{
    private static readonly TimeSpan _step = TimeSpan.FromSeconds(10);

    /// <summary>FloorTo on a positive value below a step boundary rounds down to the prior multiple.</summary>
    [Fact]
    public void FloorTo_PositiveBelowMultiple_RoundsDown() =>
        TimeSpan.FromSeconds(13).FloorTo(_step).Is(TimeSpan.FromSeconds(10));

    /// <summary>FloorTo on an exact multiple of the step returns the same value.</summary>
    [Fact]
    public void FloorTo_ExactMultiple_ReturnsSameValue() =>
        TimeSpan.FromSeconds(20).FloorTo(_step).Is(TimeSpan.FromSeconds(20));

    /// <summary>FloorTo on a negative value floors mathematically (toward -infinity).</summary>
    [Fact]
    public void FloorTo_NegativeBelowMultiple_RoundsDownAwayFromZero() =>
        TimeSpan.FromSeconds(-13).FloorTo(_step).Is(TimeSpan.FromSeconds(-20));

    /// <summary>FloorTo on a negative exact multiple returns the same value.</summary>
    [Fact]
    public void FloorTo_NegativeExactMultiple_ReturnsSameValue() =>
        TimeSpan.FromSeconds(-20).FloorTo(_step).Is(TimeSpan.FromSeconds(-20));

    /// <summary>FloorTo throws on a zero duration.</summary>
    [Fact]
    public void FloorTo_ZeroDuration_Throws() =>
        Wrap.It(() => TimeSpan.FromSeconds(5).FloorTo(TimeSpan.Zero)).Throws<ArgumentOutOfRangeException>();

    /// <summary>FloorTo throws on a negative duration.</summary>
    [Fact]
    public void FloorTo_NegativeDuration_Throws() =>
        Wrap.It(() => TimeSpan.FromSeconds(5).FloorTo(TimeSpan.FromSeconds(-1))).Throws<ArgumentOutOfRangeException>();

    /// <summary>RoundTo on a positive value below the midpoint rounds down.</summary>
    [Fact]
    public void RoundTo_PositiveBelowMidpoint_RoundsDown() =>
        TimeSpan.FromSeconds(13).RoundTo(_step).Is(TimeSpan.FromSeconds(10));

    /// <summary>RoundTo on a positive value above the midpoint rounds up.</summary>
    [Fact]
    public void RoundTo_PositiveAboveMidpoint_RoundsUp() =>
        TimeSpan.FromSeconds(16).RoundTo(_step).Is(TimeSpan.FromSeconds(20));

    /// <summary>RoundTo on a positive value exactly at the midpoint rounds away from zero (banker-free).</summary>
    [Fact]
    public void RoundTo_PositiveExactMidpoint_RoundsUp() =>
        TimeSpan.FromSeconds(15).RoundTo(_step).Is(TimeSpan.FromSeconds(20));

    /// <summary>RoundTo on a negative value below the midpoint rounds toward zero.</summary>
    [Fact]
    public void RoundTo_NegativeAboveMidpoint_RoundsTowardZero() =>
        TimeSpan.FromSeconds(-13).RoundTo(_step).Is(TimeSpan.FromSeconds(-10));

    /// <summary>RoundTo on a negative value below the midpoint rounds away from zero.</summary>
    [Fact]
    public void RoundTo_NegativeBelowMidpoint_RoundsAwayFromZero() =>
        TimeSpan.FromSeconds(-16).RoundTo(_step).Is(TimeSpan.FromSeconds(-20));

    /// <summary>CeilTo on a positive value above a multiple rounds up.</summary>
    [Fact]
    public void CeilTo_PositiveAboveMultiple_RoundsUp() =>
        TimeSpan.FromSeconds(13).CeilTo(_step).Is(TimeSpan.FromSeconds(20));

    /// <summary>CeilTo on an exact multiple returns the same value.</summary>
    [Fact]
    public void CeilTo_ExactMultiple_ReturnsSameValue() =>
        TimeSpan.FromSeconds(20).CeilTo(_step).Is(TimeSpan.FromSeconds(20));

    /// <summary>CeilTo on a negative value below the previous multiple rounds toward zero.</summary>
    [Fact]
    public void CeilTo_NegativeBelowMultiple_RoundsTowardZero() =>
        TimeSpan.FromSeconds(-13).CeilTo(_step).Is(TimeSpan.FromSeconds(-10));

    /// <summary>CeilTo on a negative exact multiple returns the same value.</summary>
    [Fact]
    public void CeilTo_NegativeExactMultiple_ReturnsSameValue() =>
        TimeSpan.FromSeconds(-20).CeilTo(_step).Is(TimeSpan.FromSeconds(-20));

    /// <summary>CeilTo throws on a zero duration.</summary>
    [Fact]
    public void CeilTo_ZeroDuration_Throws() =>
        Wrap.It(() => TimeSpan.FromSeconds(5).CeilTo(TimeSpan.Zero)).Throws<ArgumentOutOfRangeException>();

    /// <summary>FloorToSecond rounds down to the nearest second.</summary>
    [Fact]
    public void FloorToSecond_RoundsDownToSecond() =>
        TimeSpan.FromMilliseconds(1700).FloorToSecond().Is(TimeSpan.FromSeconds(1));

    /// <summary>RoundToMinute rounds to the nearest minute.</summary>
    [Fact]
    public void RoundToMinute_RoundsToNearestMinute() =>
        TimeSpan.FromSeconds(90).RoundToMinute().Is(TimeSpan.FromMinutes(2));

    /// <summary>CeilToHour rounds up to the next hour.</summary>
    [Fact]
    public void CeilToHour_RoundsUpToNextHour() => TimeSpan.FromMinutes(61).CeilToHour().Is(TimeSpan.FromHours(2));

    /// <summary>FloorToDay rounds down to the day.</summary>
    [Fact]
    public void FloorToDay_RoundsDownToDay() => TimeSpan.FromHours(25).FloorToDay().Is(TimeSpan.FromDays(1));
}
