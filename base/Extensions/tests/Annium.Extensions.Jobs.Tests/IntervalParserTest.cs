using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Jobs.Internal;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Extensions.Jobs.Tests;

/// <summary>
/// Test class for interval parser functionality
/// </summary>
public class IntervalParserTest
{
    /// <summary>
    /// Tests that the parser works with always-running intervals
    /// </summary>
    [Fact]
    public void Always_Works()
    {
        // arrange
        var parser = GetParser();

        // act - every second
        var resolver = parser.GetDelayResolver(Interval.Secondly);

        // assert - every second
        resolver(GetDate(1, 0, 0, 15)).Is(Zero);
    }

    /// <summary>
    /// Tests that the parser works with interval patterns
    /// </summary>
    [Fact]
    public void Interval_Works()
    {
        // arrange
        var parser = GetParser();

        // act
        var resolver = parser.GetDelayResolver("*/3 * * * *");

        // assert
        resolver(GetDate(1, 0, 0, 0)).Is(Sec(0));
        resolver(GetDate(1, 0, 0, 13)).Is(Sec(2));
        resolver(GetDate(1, 0, 0, 59)).Is(Sec(1));
    }

    /// <summary>
    /// Tests that the parser works with constant values
    /// </summary>
    [Fact]
    public void Const_Works()
    {
        // arrange
        var parser = GetParser();

        // act
        var resolver = parser.GetDelayResolver("27 * * * *");

        // assert
        resolver(GetDate(1, 0, 0, 0)).Is(Sec(27));
        resolver(GetDate(1, 0, 0, 13)).Is(Sec(14));
        resolver(GetDate(1, 0, 0, 49)).Is(Sec(38));
        resolver(GetDate(1, 0, 0, 59)).Is(Sec(28));
    }

    /// <summary>
    /// Tests that the parser works with list patterns
    /// </summary>
    [Fact]
    public void List_Works()
    {
        // arrange
        var parser = GetParser();

        // act
        var resolver = parser.GetDelayResolver("17,31,52 * * * *");

        // assert
        resolver(GetDate(1, 0, 0, 0)).Is(Sec(17));
        resolver(GetDate(1, 0, 0, 13)).Is(Sec(4));
        resolver(GetDate(1, 0, 0, 23)).Is(Sec(8));
        resolver(GetDate(1, 0, 0, 49)).Is(Sec(3));
        resolver(GetDate(1, 0, 0, 59)).Is(Sec(18));
    }

    /// <summary>
    /// Tests that the parser works with simple combination patterns
    /// </summary>
    [Fact]
    public void ComboSimple_Works()
    {
        // arrange
        var parser = GetParser();

        // act
        var resolver = parser.GetDelayResolver("10,50 */3 * * *");

        // assert
        resolver(GetDate(1, 0, 0, 0)).Is(Sec(10));
        resolver(GetDate(1, 0, 0, 23)).Is(Sec(27));
        resolver(GetDate(1, 0, 0, 59)).Is(Min(2) + Sec(11));
    }

    /// <summary>
    /// Regression test for day-of-month parsing at the boundaries. Previously the bounds were
    /// <c>(min=0, max=29, size=30)</c> while <see cref="LocalDateTime.Day"/> is 1-based (1..31),
    /// so cron expressions pinning days 30 or 31 silently never fired and day 0 was wrongly
    /// accepted. After the fix, bounds are <c>(1, 31, 31)</c> and expressions like
    /// <c>"0 0 0 31 *"</c> compile and resolve correctly.
    /// </summary>
    [Fact]
    public void DayOfMonth_31_Works()
    {
        // arrange
        var parser = GetParser();

        // act — "at 00:00:00 on day 31 of month"; 5-part cron is "second minute hour day day-of-week"
        var resolver = parser.GetDelayResolver("0 0 0 31 *");

        // assert — from January 2000 (year 2000 January has 31 days)
        // day 30, 23:00:00 → 1 hour until day 31, 00:00:00
        resolver(GetDate(30, 23, 0, 0)).Is(Hour(1));
        // day 31, 00:00:00 → already aligned, zero delay
        resolver(GetDate(31, 0, 0, 0)).Is(Zero);
        // negative assertion: on day 1 of a month, the delay to day 31 is NOT zero. This would
        // silently resolve to zero under the buggy (0, 29, 30) bounds where 31 was out of range.
        resolver(GetDate(1, 0, 0, 0)).Is(Day(30));
    }

    /// <summary>
    /// Tests that the parser works with full combination patterns
    /// </summary>
    [Fact]
    public void ComboFull_Works()
    {
        // arrange
        var parser = GetParser();

        // act
        var resolver = parser.GetDelayResolver("10,50 */3 2 * 1");

        // assert
        resolver(GetDate(1, 0, 0, 0)).Is(Day(2) + Hour(2) + Sec(10));
        resolver(GetDate(2, 3, 20, 43)).Is(Hour(23) + Min(1) + Sec(7));
        resolver(GetDate(3, 3, 20, 43)).Is(Day(6) + Hour(23) + Min(1) + Sec(7));
    }

    /// <summary>
    /// Gets a LocalDateTime for the specified date components
    /// </summary>
    /// <param name="day">The day of the month</param>
    /// <param name="hour">The hour of the day</param>
    /// <param name="minute">The minute of the hour</param>
    /// <param name="second">The second of the minute</param>
    /// <returns>A LocalDateTime representing the specified date and time</returns>
    private static LocalDateTime GetDate(int day, int hour, int minute, int second) =>
        new(2000, 1, day, hour, minute, second);

    /// <summary>
    /// Gets a zero duration
    /// </summary>
    private static Duration Zero { get; } = Duration.Zero;

    /// <summary>
    /// Creates a duration representing the specified number of days
    /// </summary>
    /// <param name="x">The number of days</param>
    /// <returns>A Duration representing the specified days</returns>
    private static Duration Day(int x) => Duration.FromDays(x);

    /// <summary>
    /// Creates a duration representing the specified number of hours
    /// </summary>
    /// <param name="x">The number of hours</param>
    /// <returns>A Duration representing the specified hours</returns>
    private static Duration Hour(int x) => Duration.FromHours(x);

    /// <summary>
    /// Creates a duration representing the specified number of minutes
    /// </summary>
    /// <param name="x">The number of minutes</param>
    /// <returns>A Duration representing the specified minutes</returns>
    private static Duration Min(int x) => Duration.FromMinutes(x);

    /// <summary>
    /// Creates a duration representing the specified number of seconds
    /// </summary>
    /// <param name="x">The number of seconds</param>
    /// <returns>A Duration representing the specified seconds</returns>
    private static Duration Sec(int x) => Duration.FromSeconds(x);

    /// <summary>
    /// A modulo that is not a number is reported with the token that was actually typed. Reporting the
    /// parse result instead named 0 every time - the one value the caller already knows is not theirs.
    /// </summary>
    [Fact]
    public void Modulo_NotANumber_NamesTheToken()
    {
        // arrange
        var parser = GetParser();

        // act & assert
        var error = Wrap.It(() => parser.GetDelayResolver("*/x * * * *")).Throws<ArgumentException>();
        error.Message.Contains("'x'").IsTrue("the message must quote the token that was not understood");
    }

    /// <summary>
    /// Gets an interval parser instance for testing
    /// </summary>
    /// <returns>An IIntervalParser instance</returns>
    private static IIntervalParser GetParser() =>
        new ServiceContainer().AddScheduler().BuildServiceProvider().Resolve<IIntervalParser>();
}
