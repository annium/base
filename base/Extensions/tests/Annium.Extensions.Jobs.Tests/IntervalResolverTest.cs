using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Jobs.Internal;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Extensions.Jobs.Tests;

public class IntervalResolverTest
{
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

    private IIntervalResolver GetResolver() => new ServiceContainer()
        .AddScheduler()
        .BuildServiceProvider()
        .Resolve<IIntervalResolver>();

    private Instant GetInstant(int year, int month, int day, int hour, int minute, int second) =>
        Instant.FromDateTimeUtc(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc));
}