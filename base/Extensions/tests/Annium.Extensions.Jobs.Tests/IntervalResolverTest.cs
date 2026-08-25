using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Jobs.Internal;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Extensions.Jobs.Tests;

/// <summary>
/// Test class for interval resolver functionality
/// </summary>
public class IntervalResolverTest
{
    /// <summary>
    /// Tests that minutely intervals work correctly
    /// </summary>
    [Fact]
    public void Minutely_Works()
    {
        // arrange
        var resolver = GetResolver();

        // act
        var isMatch = resolver.GetMatcher("* * * * *");

        // assert
        isMatch(GetInstant(2000, 1, 12, 5, 6, 15)).IsFalse();
        isMatch(GetInstant(2000, 1, 12, 5, 6, 0)).IsTrue();
        isMatch(GetInstant(2000, 1, 12, 5, 7, 0)).IsTrue();
    }

    /// <summary>
    /// Tests that normal interval patterns work correctly
    /// </summary>
    [Fact]
    public void NormalInterval_Works()
    {
        // arrange
        var resolver = GetResolver();

        // act
        var isMatch = resolver.GetMatcher("0 0 * * *");

        // assert
        isMatch(GetInstant(2000, 1, 12, 5, 6, 0)).IsFalse();
        isMatch(GetInstant(2000, 1, 12, 0, 0, 0)).IsTrue();
    }

    /// <summary>
    /// Tests that list interval patterns work correctly
    /// </summary>
    [Fact]
    public void ListInterval_Works()
    {
        // arrange
        var resolver = GetResolver();

        // act
        var isMatch = resolver.GetMatcher("0,30 * * * *");

        // assert
        isMatch(GetInstant(2000, 1, 12, 5, 6, 0)).IsFalse();
        isMatch(GetInstant(2000, 1, 12, 0, 0, 0)).IsTrue();
        isMatch(GetInstant(2000, 1, 12, 0, 30, 0)).IsTrue();
    }

    /// <summary>
    /// A day-of-month schedule matches that calendar day. ZonedDateTime.Day is 1-based, so bounds that are
    /// not make the last day of the month unschedulable.
    /// </summary>
    [Fact]
    public void DayOfMonth_LastDay_Works()
    {
        // arrange
        var resolver = GetResolver();

        // act
        var isMatch = resolver.GetMatcher("0 0 31 * *");

        // assert
        isMatch(GetInstant(2000, 1, 31, 0, 0, 0)).IsTrue();
        isMatch(GetInstant(2000, 1, 30, 0, 0, 0)).IsFalse();
    }

    /// <summary>
    /// The same holds for months: December is the twelfth, not the eleventh.
    /// </summary>
    [Fact]
    public void Month_December_Works()
    {
        // arrange
        var resolver = GetResolver();

        // act
        var isMatch = resolver.GetMatcher("0 0 1 12 *");

        // assert
        isMatch(GetInstant(2000, 12, 1, 0, 0, 0)).IsTrue();
        isMatch(GetInstant(2000, 11, 1, 0, 0, 0)).IsFalse();
    }

    /// <summary>
    /// Gets an interval resolver instance for testing
    /// </summary>
    /// <returns>An IIntervalResolver instance</returns>
    private IIntervalResolver GetResolver() =>
        new ServiceContainer().AddScheduler().BuildServiceProvider().Resolve<IIntervalResolver>();

    /// <summary>
    /// Gets an Instant for the specified date and time components
    /// </summary>
    /// <param name="year">The year</param>
    /// <param name="month">The month</param>
    /// <param name="day">The day</param>
    /// <param name="hour">The hour</param>
    /// <param name="minute">The minute</param>
    /// <param name="second">The second</param>
    /// <returns>An Instant representing the specified date and time</returns>
    private Instant GetInstant(int year, int month, int day, int hour, int minute, int second) =>
        Instant.FromDateTimeUtc(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc));
}
