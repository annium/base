using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Annium.Logging.Shared;

namespace Annium.Seq.Logging.Internal;

/// <summary>
/// Static factory class for creating Compact Log Event Format (CLEF) formatters for Seq logging.
/// Provides functionality to format log messages according to CLEF specification with context properties.
/// </summary>
/// <typeparam name="TContext">The type of the logging context containing additional properties</typeparam>
internal static class CompactLogEvent<TContext>
    where TContext : class
{
    /// <summary>
    /// Cached dictionary of context type properties mapped to their snake_case names for efficient property access during formatting.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, PropertyInfo> _properties = typeof(TContext)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(x => x.CanRead)
        .ToDictionary(x => x.Name.SnakeCase());

    /// <summary>
    /// Creates a formatter function that converts log messages to Compact Log Event Format (CLEF) dictionaries.
    /// The formatter includes standard CLEF fields (@t, @m, @mt, @l, @x) and custom properties from context and data.
    /// </summary>
    /// <param name="project">The project identifier to include in the @p field of CLEF events</param>
    /// <returns>A function that transforms log messages into CLEF-formatted dictionaries</returns>
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

            foreach (var (name, property) in _properties)
            {
                var value = property.GetValue(m.Context);
                if (value is not null)
                    result.TryAdd(name, value.ToString());
            }

            return result;
        };

    /// <summary>
    /// Builds a message prefix containing thread ID, subject information, and optional source location details.
    /// </summary>
    /// <param name="m">The log message to build the prefix for</param>
    /// <returns>A formatted prefix string in the format "[ThreadId] SubjectType#SubjectId at Type.Member:Line >> "</returns>
    private static string BuildMessagePrefix(LogMessage<TContext> m)
    {
        var sb = new StringBuilder();
        sb.Append($"{m.SubjectType}#{m.SubjectId}");
        if (m.Line != 0)
            sb.Append($" at {m.Type}.{m.Member}:{m.Line}");

        return $"[{m.ThreadId:D3}] {sb} >> ";
    }
}
