using System;
using System.Runtime.CompilerServices;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class YearMonthExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth NextYear(this YearMonth period)
        => period.AddYears(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth PrevYear(this YearMonth period)
        => period.AddYears(-1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth NextMonth(this YearMonth period)
        => period.AddMonths(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth PrevMonth(this YearMonth period)
        => period.AddMonths(-1);

    public static YearMonth AddYears(this YearMonth period, int value)
        => new(period.Era, period.Year + value, period.Month);

    public static YearMonth AddMonths(this YearMonth period, int value)
    {
        var totalMonths = period.Month + value;
        // totalMonths - 1 - hack to define YearMonth with 12 months'
        var addYears = (int)Math.Floor((decimal)(totalMonths - 1) / 12);
        var years = period.Year + addYears;
        var months = totalMonths - addYears * 12;

        return new YearMonth(period.Era, years, months);
    }
}