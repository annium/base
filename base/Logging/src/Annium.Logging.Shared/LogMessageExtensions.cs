using System;
using System.Runtime.CompilerServices;
using System.Text;
using NodaTime;

namespace Annium.Logging.Shared;

public static class LogMessageExtensions
{
    private static readonly DateTimeZone _currentTz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
    private static readonly DateTimeZone _utcTz = DateTimeZone.Utc;

    public static Func<LogMessage<TContext>, string> DefaultFormat<TContext>(Func<LogMessage<TContext>, string> time)
        where TContext : class
    {
        return m =>
        {
            var sb = new StringBuilder();
            sb.Append(m.Subject());
            if (m.Line != 0)
                sb.Append($" at {m.Location()}");

            return $"[{time(m)}] {m.Level} [{m.ThreadId:D3}] {sb} >> {m.Message}";
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Subject<TContext>(this LogMessage<TContext> m)
        where TContext : class =>
        string.IsNullOrWhiteSpace(m.SubjectId) ? m.SubjectType : $"{m.SubjectType}#{m.SubjectId}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Location<TContext>(this LogMessage<TContext> m)
        where TContext : class => $"{m.Type}.{m.Member}:{m.Line}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LocalDateTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return m.Instant.InZone(_currentTz).LocalDateTime.ToString("dd.MM.yy HH:mm:ss.fff", null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LocalTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return m.Instant.InZone(_currentTz).LocalDateTime.ToString("HH:mm:ss.fff", null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UtcDateTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return m.Instant.InZone(_utcTz).LocalDateTime.ToString("dd.MM.yy HH:mm:ss.fff", null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UtcTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return m.Instant.InZone(_utcTz).LocalDateTime.ToString("HH:mm:ss.fff", null);
    }
}
