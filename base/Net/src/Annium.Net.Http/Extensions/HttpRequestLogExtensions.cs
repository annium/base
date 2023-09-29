using System;
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
        LogData log = default
    )
        where T : ILogSubject => request.Intercept(async next =>
    {
        var id = Guid.NewGuid();
        var response = default(IHttpResponse);
        try
        {
            subject.Trace(
                "request {id}: {method} {uri}",
                id,
                request.Method,
                request.Uri
            );

            if (log.HasFlag(LogData.Headers))
            {
                foreach (var (name, values) in request.Headers)
                    subject.Trace<string, string>("- {headerName}: {headerValues}", name, string.Join(", ", values));
            }

            response = await next();

            return response;
        }
        catch (Exception e)
        {
            subject.Trace(
                "failed {id}: {method} {uri}: {e}",
                id,
                request.Method,
                request.Uri,
                e
            );
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
                    foreach (var (name, values) in response.Headers)
                        subject.Trace<string, string>("- {headerName}: {headerValues}", name, string.Join(", ", values));
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