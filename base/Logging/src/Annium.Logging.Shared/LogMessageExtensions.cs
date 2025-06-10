using System;
using System.Runtime.CompilerServices;
using System.Text;
using NodaTime;

namespace Annium.Logging.Shared;

/// <summary>
/// Extension methods for LogMessage to provide formatting and time zone conversion functionality
/// </summary>
public static class LogMessageExtensions
{
    /// <summary>
    /// The current system time zone
    /// </summary>
    private static readonly DateTimeZone _currentTz = DateTimeZoneProviders.Tzdb.GetSystemDefault();

    /// <summary>
    /// The UTC time zone
    /// </summary>
    private static readonly DateTimeZone _utcTz = DateTimeZone.Utc;

    /// <summary>
    /// Creates a default formatter function for log messages
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="time">Function to format the time portion of the message</param>
    /// <returns>A formatter function for log messages</returns>
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

    /// <summary>
    /// Gets the subject string representation (type and optional ID)
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted subject string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Subject<TContext>(this LogMessage<TContext> m)
        where TContext : class =>
        string.IsNullOrWhiteSpace(m.SubjectId) ? m.SubjectType : $"{m.SubjectType}#{m.SubjectId}";

    /// <summary>
    /// Gets the location string representation (type.member:line)
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted location string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Location<TContext>(this LogMessage<TContext> m)
        where TContext : class => $"{m.Type}.{m.Member}:{m.Line}";

    /// <summary>
    /// Formats the log message timestamp as local date and time
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted local date and time string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LocalDateTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return m.Instant.InZone(_currentTz).LocalDateTime.ToString("dd.MM.yy HH:mm:ss.fff", null);
    }

    /// <summary>
    /// Formats the log message timestamp as local time only
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted local time string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string LocalTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return m.Instant.InZone(_currentTz).LocalDateTime.ToString("HH:mm:ss.fff", null);
    }

    /// <summary>
    /// Formats the log message timestamp as UTC date and time
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted UTC date and time string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UtcDateTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return m.Instant.InZone(_utcTz).LocalDateTime.ToString("dd.MM.yy HH:mm:ss.fff", null);
    }

    /// <summary>
    /// Formats the log message timestamp as UTC time only
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="m">The log message</param>
    /// <returns>A formatted UTC time string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UtcTime<TContext>(LogMessage<TContext> m)
        where TContext : class
    {
        return m.Instant.InZone(_utcTz).LocalDateTime.ToString("HH:mm:ss.fff", null);
    }
}
