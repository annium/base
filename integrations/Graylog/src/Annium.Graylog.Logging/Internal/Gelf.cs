using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Logging.Shared;

namespace Annium.Graylog.Logging.Internal;

internal static class Gelf
{
    public static Func<LogMessage<TContext>, IReadOnlyDictionary<string, object?>> CreateFormat<TContext>(
        string project
    )
        where TContext : class, ILogContext =>
        m =>
        {
            var result = new Dictionary<string, object?>
            {
                ["host"] = project,
                ["short_message"] = m.Message,
                ["timestamp"] = m.Instant.ToUnixTimeMilliseconds() / 1000m,
                ["level"] = MapLogLevel(m.Level),
                ["_subject"] = $"{m.SubjectType}#{m.SubjectId}",
                ["_source"] = $"{m.Type}.{m.Member}:{m.Line}",
                ["_thread"] = m.ThreadId,
            };
            if (m.Exception is not null)
                result["full_message"] = $"{m.Exception.Message}{m.Exception.StackTrace}";

            foreach (var (key, value) in m.Data)
                result.TryAdd($"_{key}", value?.ToString());

            return result;
        };

    private static int MapLogLevel(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => 7,
            LogLevel.Debug => 6,
            LogLevel.Info => 5,
            LogLevel.Warn => 4,
            LogLevel.Error => 3,
            _ => 3
        };
}
