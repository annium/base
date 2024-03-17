using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class HttpRequestLogExtensions
{
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
                subject.Trace("request {id}: {method} {uri}", id, request.Method, request.Uri);

                if (log.HasFlag(LogData.Headers))
                {
                    IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = headerMasks is null
                        ? request.Headers
                        : request.Headers.Where(
                            x => headerMasks.Any(m => x.Key.Contains(m, StringComparison.InvariantCultureIgnoreCase))
                        );
                    foreach (var (name, values) in headers)
                        subject.Trace<string, string>(
                            "- {headerName}: {headerValues}",
                            name,
                            string.Join(", ", values)
                        );
                }

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
                    subject.Trace<Guid, HttpMethod, Uri, HttpStatusCode, string>(
                        "response {id}: {method} {uri} -> {statusCode} ({statusText})",
                        id,
                        request.Method,
                        request.Uri,
                        response.StatusCode,
                        response.StatusText
                    );

                    if (log.HasFlag(LogData.Headers))
                    {
                        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = headerMasks is null
                            ? response.Headers
                            : response.Headers.Where(
                                x =>
                                    headerMasks.Any(m => x.Key.Contains(m, StringComparison.InvariantCultureIgnoreCase))
                            );
                        foreach (var (name, values) in headers)
                            subject.Trace<string, string>(
                                "- {headerName}: {headerValues}",
                                name,
                                string.Join(", ", values)
                            );
                    }

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
