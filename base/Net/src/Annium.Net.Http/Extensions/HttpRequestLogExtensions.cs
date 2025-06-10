using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Annium.Logging;

namespace Annium.Net.Http.Extensions;

/// <summary>
/// Extension methods for adding logging to HTTP requests
/// </summary>
public static class HttpRequestLogExtensions
{
    /// <summary>
    /// Adds logging capabilities to the HTTP request
    /// </summary>
    /// <typeparam name="T">The type of the log subject</typeparam>
    /// <param name="request">The HTTP request</param>
    /// <param name="subject">The log subject to write logs to</param>
    /// <param name="log">The type of data to log</param>
    /// <param name="headerMasks">Header names to include in logging</param>
    /// <returns>The HTTP request with logging interceptor attached</returns>
    public static IHttpRequest WithLogFrom<T>(
        this IHttpRequest request,
        T subject,
        LogData log = default,
        string[]? headerMasks = null
    )
        where T : ILogSubject =>
        request.Intercept(async next =>
        {
            var id = Guid.NewGuid();
            var response = default(IHttpResponse);
            try
            {
                var headers = string.Empty;
                if (log.HasFlag(LogData.Headers))
                {
                    var sb = new StringBuilder($"{Environment.NewLine}Headers:");
                    IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerPairs = headerMasks is null
                        ? request.Headers
                        : request.Headers.Where(x =>
                            headerMasks.Any(m => x.Key.Contains(m, StringComparison.InvariantCultureIgnoreCase))
                        );
                    foreach (var (name, values) in headerPairs)
                        sb.AppendLine($"- {name}: {string.Join(", ", values)}");
                    headers = sb.ToString();
                }

                subject.Trace<Guid, HttpMethod, Uri, string>(
                    "request {id}: {method} {uri}{headers}",
                    id,
                    request.Method,
                    request.Uri,
                    headers
                );

                response = await next();

                return response;
            }
            catch (Exception e)
            {
                subject.Trace("failed {id}: {method} {uri}: {e}", id, request.Method, request.Uri, e);
                throw;
            }
            finally
            {
                if (response is not null)
                {
                    var headers = string.Empty;
                    if (log.HasFlag(LogData.Headers))
                    {
                        var sb = new StringBuilder($"{Environment.NewLine}Headers:");
                        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerPairs = headerMasks is null
                            ? response.Headers
                            : response.Headers.Where(x =>
                                headerMasks.Any(m => x.Key.Contains(m, StringComparison.InvariantCultureIgnoreCase))
                            );
                        foreach (var (name, values) in headerPairs)
                            sb.AppendLine($"- {name}: {string.Join(", ", values)}");
                        headers = sb.ToString();
                    }
                    subject.Trace<Guid, HttpMethod, Uri, HttpStatusCode, string, string>(
                        "response {id}: {method} {uri} -> {statusCode} ({statusText}){headers}",
                        id,
                        request.Method,
                        request.Uri,
                        response.StatusCode,
                        response.StatusText,
                        headers
                    );

                    if (log.HasFlag(LogData.Response))
                        subject.Trace(await response.Content.ReadAsStringAsync());
                }
            }
        });
}

[Flags]
public enum LogData
{
    Headers = 1 << 0,
    Response = 1 << 1,
}
