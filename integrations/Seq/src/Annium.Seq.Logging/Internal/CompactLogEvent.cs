using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Annium.Logging.Shared;

namespace Annium.Seq.Logging.Internal;

internal static class CompactLogEvent<TContext>
    where TContext : class
{
    private static readonly IReadOnlyDictionary<string, PropertyInfo> Properties = typeof(TContext)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(x => x.CanRead)
        .ToDictionary(x => x.Name.SnakeCase());

    public static Func<LogMessage<TContext>, IReadOnlyDictionary<string, string?>> CreateFormat(string project) =>
        m =>
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

            foreach (var (name, property) in Properties)
            {
                var value = property.GetValue(m.Context);
                if (value is not null)
                    result.TryAdd(name, value.ToString());
            }

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
