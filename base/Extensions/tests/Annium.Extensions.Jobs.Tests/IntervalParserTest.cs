using Annium.Core.DependencyInjection;
using Annium.Extensions.Jobs.Internal;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Extensions.Jobs.Tests;

public class IntervalParserTest
{
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

    private static LocalDateTime GetDate(int day, int hour, int minute, int second) =>
        new(2000, 1, day, hour, minute, second);

    private static Duration Zero { get; } = Duration.Zero;
    private static Duration Day(int x) => Duration.FromDays(x);
    private static Duration Hour(int x) => Duration.FromHours(x);
    private static Duration Min(int x) => Duration.FromMinutes(x);
    private static Duration Sec(int x) => Duration.FromSeconds(x);

    private static IIntervalParser GetParser() => new ServiceContainer()
        .AddScheduler()
        .BuildServiceProvider()
        .Resolve<IIntervalParser>();
}