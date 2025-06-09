using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Annium.Logging;
using Annium.Logging.Shared;

namespace Annium.Graylog.Logging.Internal;

/// <summary>
/// Static utility class that provides GELF (Graylog Extended Log Format) message formatting and conversion functionality for structured logging.
/// </summary>
/// <typeparam name="TContext">The type of the logging context that provides additional contextual information for log messages.</typeparam>
internal static class Gelf<TContext>
    where TContext : class
{
    /// <summary>
    /// Pre-computed dictionary of context type properties mapped to snake_case field names for efficient GELF field extraction.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, PropertyInfo> _properties = typeof(TContext)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(x => x.CanRead)
        .ToDictionary(x => $"_{x.Name.SnakeCase()}");

    /// <summary>
    /// Creates a formatter function that converts log messages into GELF-compliant dictionaries for transmission to Graylog.
    /// </summary>
    /// <param name="project">The project name to be used as the host identifier in GELF messages.</param>
    /// <returns>A function that transforms log messages into GELF format dictionaries containing all required and custom fields.</returns>
    public static Func<LogMessage<TContext>, IReadOnlyDictionary<string, object?>> CreateFormat(string project) =>
        m =>
        {
            var result = new Dictionary<string, object?>
            {
                ["host"] = project,
                ["short_message"] = BuildMessage(m, m.Message),
                ["timestamp"] = m.Instant.ToUnixTimeMilliseconds() / 1000m,
                ["level"] = MapLogLevel(m.Level),
                ["_log_level"] = MapLogLevelText(m.Level),
                ["_subject"] = m.SubjectType,
                ["_subject_id"] = m.SubjectId,
                ["_source_type"] = m.Type,
                ["_source_member"] = $"{m.Member}:{m.Line}",
                ["_thread"] = m.ThreadId,
            };
            if (m.Exception is not null)
                result["full_message"] = BuildMessage(m, $"{m.Exception.Message}{m.Exception.StackTrace}");

            foreach (var (key, value) in m.Data)
                result.TryAdd($"_{key}", value?.ToString());

            foreach (var (name, property) in _properties)
            {
                var value = property.GetValue(m.Context);
                if (value is not null)
                    result.TryAdd(name, value.ToString());
            }

            return result;
        };

    /// <summary>
    /// Builds a formatted log message string that includes subject identification, source location, and thread information.
    /// </summary>
    /// <param name="m">The log message containing source and context information.</param>
    /// <param name="text">The primary text content to be included in the formatted message.</param>
    /// <returns>A formatted message string with thread ID, subject type and ID, source location, and the provided text.</returns>
    private static string BuildMessage(LogMessage<TContext> m, string text)
    {
        var sb = new StringBuilder($"{m.SubjectType}#{m.SubjectId}");
        if (m.Line != 0)
            sb.Append($" at {m.Type}.{m.Member}:{m.Line}");

        return $"[{m.ThreadId:D3}] {sb} >> {text}";
    }

    /// <summary>
    /// Maps framework log levels to corresponding GELF/Syslog numeric severity levels for standardized log level representation.
    /// </summary>
    /// <param name="level">The framework log level to be converted.</param>
    /// <returns>A numeric severity level where 7 is debug/trace, 6 is informational, 5 is notice, 4 is warning, and 3 is error.</returns>
    private static int MapLogLevel(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => 7,
            LogLevel.Debug => 6,
            LogLevel.Info => 5,
            LogLevel.Warn => 4,
            LogLevel.Error => 3,
            _ => 3,
        };

    /// <summary>
    /// Converts framework log levels to their corresponding string representations for human-readable log level identification.
    /// </summary>
    /// <param name="level">The framework log level to be converted to text.</param>
    /// <returns>The string name of the log level, or "None" for unrecognized levels.</returns>
    private static string MapLogLevelText(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => nameof(LogLevel.Trace),
            LogLevel.Debug => nameof(LogLevel.Debug),
            LogLevel.Info => nameof(LogLevel.Info),
            LogLevel.Warn => nameof(LogLevel.Warn),
            LogLevel.Error => nameof(LogLevel.Error),
            _ => nameof(LogLevel.None),
        };
}
