using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class YearMonthExtensionsTest
{
    [Fact]
    public void AddYears()
    {
        // assert
        new YearMonth(1, 2).AddYears(2).Is(new YearMonth(3, 2));
        new YearMonth(3, 2).AddYears(-2).Is(new YearMonth(1, 2));
    }

    [Fact]
    public void AddMonths()
    {
        // assert
        new YearMonth(1, 10).AddMonths(13).Is(new YearMonth(2, 11));
        new YearMonth(1, 10).AddMonths(14).Is(new YearMonth(2, 12));
        new YearMonth(1, 10).AddMonths(15).Is(new YearMonth(3, 1));
        new YearMonth(3, 1).AddMonths(-12).Is(new YearMonth(2, 1));
        new YearMonth(3, 1).AddMonths(-13).Is(new YearMonth(1, 12));
        new YearMonth(3, 1).AddMonths(-14).Is(new YearMonth(1, 11));
    }

    [Fact]
    public void NextYear()
    {
        // assert
        new YearMonth(1, 1).NextYear().Is(new YearMonth(2, 1));
    }

    [Fact]
    public void PrevYear()
    {
        // assert
        new YearMonth(2, 1).PrevYear().Is(new YearMonth(1, 1));
    }

    [Fact]
    public void NextMonth()
    {
        // assert
        new YearMonth(1, 1).NextMonth().Is(new YearMonth(1, 2));
        new YearMonth(1, 12).NextMonth().Is(new YearMonth(2, 1));
    }

    [Fact]
    public void PrevMonth()
    {
        // assert
        new YearMonth(1, 2).PrevMonth().Is(new YearMonth(1, 1));
        new YearMonth(2, 1).PrevMonth().Is(new YearMonth(1, 12));
    }
}