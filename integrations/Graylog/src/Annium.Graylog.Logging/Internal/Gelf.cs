using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Annium.Logging;
using Annium.Logging.Shared;

namespace Annium.Graylog.Logging.Internal;

internal static class Gelf<TContext>
    where TContext : class
{
    private static readonly IReadOnlyDictionary<string, PropertyInfo> _properties = typeof(TContext)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(x => x.CanRead)
        .ToDictionary(x => $"_{x.Name.SnakeCase()}");

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

    private static string BuildMessage(LogMessage<TContext> m, string text)
    {
        var sb = new StringBuilder($"{m.SubjectType}#{m.SubjectId}");
        if (m.Line != 0)
            sb.Append($" at {m.Type}.{m.Member}:{m.Line}");

        return $"[{m.ThreadId:D3}] {sb} >> {text}";
    }

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
