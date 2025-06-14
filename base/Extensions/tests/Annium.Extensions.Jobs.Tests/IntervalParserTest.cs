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
    /// Gets an interval parser instance for testing
    /// </summary>
    /// <returns>An IIntervalParser instance</returns>
    private static IIntervalParser GetParser() =>
        new ServiceContainer().AddScheduler().BuildServiceProvider().Resolve<IIntervalParser>();
}
