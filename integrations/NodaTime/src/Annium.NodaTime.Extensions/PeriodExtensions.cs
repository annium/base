using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class PeriodExtensions
{
    public static Period FloorToSecond(this Period m) =>
        m.FloorTo(Period.FromSeconds(1));

    public static Period FloorToMinute(this Period m) =>
        m.FloorTo(Period.FromMinutes(1));

    public static Period FloorToHour(this Period m) =>
        m.FloorTo(Period.FromHours(1));

    public static Period FloorToDay(this Period m) =>
        m.FloorTo(Period.FromDays(1));

    public static Period FloorTo(this Period m, Period d)
    {
        var mt = (long)m.ToDuration().TotalTicks;
        var dt = (long)d.ToDuration().TotalTicks;

        return Period.FromTicks(mt - mt % dt);
    }

    public static Period RoundToSecond(this Period m) =>
        m.RoundTo(Period.FromSeconds(1));

    public static Period RoundToMinute(this Period m) =>
        m.RoundTo(Period.FromMinutes(1));

    public static Period RoundToHour(this Period m) =>
        m.RoundTo(Period.FromHours(1));

    public static Period RoundToDay(this Period m) =>
        m.RoundTo(Period.FromDays(1));

    public static Period RoundTo(this Period m, Period d)
    {
        var mt = (long)m.ToDuration().TotalTicks;
        var dt = (long)d.ToDuration().TotalTicks;
        var diff = mt % dt;

        return Period.FromTicks(mt - diff + (dt > diff * 2L ? 0L : dt));
    }

    public static Period CeilToSecond(this Period m) =>
        m.CeilTo(Period.FromSeconds(1));

    public static Period CeilToMinute(this Period m) =>
        m.CeilTo(Period.FromMinutes(1));

    public static Period CeilToHour(this Period m) =>
        m.CeilTo(Period.FromHours(1));

    public static Period CeilToDay(this Period m) =>
        m.CeilTo(Period.FromDays(1));

    public static Period CeilTo(this Period m, Period d)
    {
        var mt = (long)m.ToDuration().TotalTicks;
        var dt = (long)d.ToDuration().TotalTicks;

        return Period.FromTicks(mt + dt - mt % dt);
    }
}