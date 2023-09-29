using System;

namespace Annium;

public static class TimeSpanExtensions
{
    public static TimeSpan FloorToSecond(this TimeSpan m) =>
        m.FloorTo(TimeSpan.FromSeconds(1));

    public static TimeSpan FloorToMinute(this TimeSpan m) =>
        m.FloorTo(TimeSpan.FromMinutes(1));

    public static TimeSpan FloorToHour(this TimeSpan m) =>
        m.FloorTo(TimeSpan.FromHours(1));

    public static TimeSpan FloorToDay(this TimeSpan m) =>
        m.FloorTo(TimeSpan.FromDays(1));

    public static TimeSpan FloorTo(this TimeSpan t, TimeSpan d) =>
        TimeSpan.FromTicks(t.Ticks - t.Ticks % d.Ticks);

    public static TimeSpan RoundToSecond(this TimeSpan m) =>
        m.RoundTo(TimeSpan.FromSeconds(1));

    public static TimeSpan RoundToMinute(this TimeSpan m) =>
        m.RoundTo(TimeSpan.FromMinutes(1));

    public static TimeSpan RoundToHour(this TimeSpan m) =>
        m.RoundTo(TimeSpan.FromHours(1));

    public static TimeSpan RoundToDay(this TimeSpan m) =>
        m.RoundTo(TimeSpan.FromDays(1));

    public static TimeSpan RoundTo(this TimeSpan t, TimeSpan d)
    {
        var mt = t.Ticks;
        var dt = d.Ticks;
        var diff = mt % dt;

        return TimeSpan.FromTicks(mt - diff + (dt > diff * 2L ? 0L : dt));
    }

    public static TimeSpan CeilToSecond(this TimeSpan m) =>
        m.CeilTo(TimeSpan.FromSeconds(1));

    public static TimeSpan CeilToMinute(this TimeSpan m) =>
        m.CeilTo(TimeSpan.FromMinutes(1));

    public static TimeSpan CeilToHour(this TimeSpan m) =>
        m.CeilTo(TimeSpan.FromHours(1));

    public static TimeSpan CeilToDay(this TimeSpan m) =>
        m.CeilTo(TimeSpan.FromDays(1));

    public static TimeSpan CeilTo(this TimeSpan t, TimeSpan d) =>
        TimeSpan.FromTicks(t.Ticks + d.Ticks - t.Ticks % d.Ticks);
}