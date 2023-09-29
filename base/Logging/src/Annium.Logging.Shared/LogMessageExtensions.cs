using System;
using System.Runtime.CompilerServices;
using System.Text;
using NodaTime;

namespace Annium.Logging.Shared;

public static class LogMessageExtensions
{
    private static readonly DateTimeZone CurrentTz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
    private static readonly DateTimeZone UtcTz = DateTimeZone.Utc;

    public static Func<LogMessage<TContext>, string> DefaultFormat<TContext>(Func<LogMessage<TContext>, string> time)
        where TContext : class, ILogContext
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
        where TContext : class, ILogContext
        => string.IsNullOrWhiteSpace(m.SubjectId) ? m.SubjectType : $"{m.SubjectType}#{m.SubjectId}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Location<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => $"{m.Type}.{m.Member}:{m.Line}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LocalDateTime<TContext>(LogMessage<TContext> m)
        where TContext : class, ILogContext
        => m.Instant.InZone(CurrentTz).LocalDateTime.ToString("dd.MM.yy HH:mm:ss.fff", null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LocalTime<TContext>(LogMessage<TContext> m)
        where TContext : class, ILogContext
        => m.Instant.InZone(CurrentTz).LocalDateTime.ToString("HH:mm:ss.fff", null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UtcDateTime<TContext>(LogMessage<TContext> m)
        where TContext : class, ILogContext
        => m.Instant.InZone(UtcTz).LocalDateTime.ToString("dd.MM.yy HH:mm:ss.fff", null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UtcTime<TContext>(LogMessage<TContext> m)
        where TContext : class, ILogContext
        => m.Instant.InZone(UtcTz).LocalDateTime.ToString("HH:mm:ss.fff", null);
}