using System;
using System.Collections.Generic;
using System.Text;
using Annium.Logging.Shared;

namespace Annium.Logging.Seq.Internal;

internal static class CompactLogEvent<TContext>
    where TContext : class, ILogContext
{
    public static Func<LogMessage<TContext>, IReadOnlyDictionary<string, string?>> CreateFormat(string project) => m =>
    {
        var prefix = BuildMessagePrefix(m);
        var result = new Dictionary<string, string?>
        {
            ["@p"] = project,
            ["@t"] = m.Instant.InUtc().LocalDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff'Z'", null),
            ["@m"] = $"{prefix}{m.Message}",
            ["@mt"] = $"{prefix}{m.MessageTemplate}",
            ["@l"] = m.Level.ToString(),
        };
        if (m.Exception is not null)
            result["@x"] = $"{m.Exception.Message}{m.Exception.StackTrace}";

        foreach (var (key, value) in m.Data)
            result[key] = value?.ToString();

        return result;
    };

    private static string BuildMessagePrefix(LogMessage<TContext> m)
    {
        var sb = new StringBuilder();
        sb.Append($"{m.SubjectType}#{m.SubjectId}");
        if (m.Line != 0)
            sb.Append($" at {m.Type}.{m.Member}:{m.Line}");

        return $"[{m.ThreadId:D3}] {sb} >> ";
    }
}