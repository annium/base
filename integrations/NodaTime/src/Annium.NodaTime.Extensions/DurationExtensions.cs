using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class DurationExtensions
{
    public static Duration FloorToSecond(this Duration m) =>
        m.FloorTo(Duration.FromSeconds(1));

    public static Duration FloorToMinute(this Duration m) =>
        m.FloorTo(Duration.FromMinutes(1));

    public static Duration FloorToHour(this Duration m) =>
        m.FloorTo(Duration.FromHours(1));

    public static Duration FloorToDay(this Duration m) =>
        m.FloorTo(Duration.FromDays(1));

    public static Duration FloorTo(this Duration m, Duration d)
    {
        var mt = (long)m.TotalTicks;
        var dt = (long)d.TotalTicks;

        return Duration.FromTicks(mt - mt % dt);
    }

    public static Duration RoundToSecond(this Duration m) =>
        m.RoundTo(Duration.FromSeconds(1));

    public static Duration RoundToMinute(this Duration m) =>
        m.RoundTo(Duration.FromMinutes(1));

    public static Duration RoundToHour(this Duration m) =>
        m.RoundTo(Duration.FromHours(1));

    public static Duration RoundToDay(this Duration m) =>
        m.RoundTo(Duration.FromDays(1));

    public static Duration RoundTo(this Duration m, Duration d)
    {
        var mt = (long)m.TotalTicks;
        var dt = (long)d.TotalTicks;
        var diff = mt % dt;

        return Duration.FromTicks(mt - diff + (dt > diff * 2L ? 0L : dt));
    }

    public static Duration CeilToSecond(this Duration m) =>
        m.CeilTo(Duration.FromSeconds(1));

    public static Duration CeilToMinute(this Duration m) =>
        m.CeilTo(Duration.FromMinutes(1));

    public static Duration CeilToHour(this Duration m) =>
        m.CeilTo(Duration.FromHours(1));

    public static Duration CeilToDay(this Duration m) =>
        m.CeilTo(Duration.FromDays(1));

    public static Duration CeilTo(this Duration m, Duration d)
    {
        var mt = (long)m.TotalTicks;
        var dt = (long)d.TotalTicks;

        return Duration.FromTicks(mt + dt - mt % dt);
    }
}