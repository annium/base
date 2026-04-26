using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Json.Internal;

/// <summary>
/// Deferred configuration source that fetches a JSON document from a remote endpoint at
/// <see cref="LoadAsync"/> time. Honors a per-source timeout via <see cref="HttpClient.Timeout"/>.
/// </summary>
internal sealed class JsonRemoteSource : IConfigurationSource
{
    /// <summary>Default timeout when none is supplied at registration.</summary>
    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    /// <summary>The URI to fetch.</summary>
    private readonly Uri _uri;

    /// <summary>Maximum time before the HTTP call is aborted.</summary>
    private readonly TimeSpan _timeout;

    /// <summary>Whether a fetch failure (non-2xx, network error, timeout) is silenced.</summary>
    public bool Optional { get; }

    public JsonRemoteSource(Uri uri, bool optional, TimeSpan? timeout)
    {
        _uri = uri;
        _timeout = timeout ?? DefaultTimeout;
        Optional = optional;
    }

    /// <summary>
    /// Issues a GET to the configured URI and flattens the JSON response. Translates
    /// <see cref="TaskCanceledException"/> to <see cref="TimeoutException"/> when the per-source
    /// timeout is the cancellation cause, so the caller can distinguish.
    /// </summary>
    /// <param name="ct">Cancellation token forwarded by <c>BuildAsync</c>.</param>
    /// <returns>Flattened JSON configuration.</returns>
    public async ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct)
    {
        using var client = new HttpClient { Timeout = _timeout };
        try
        {
            using var response = await client.GetAsync(_uri, ct);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Json configuration not available at {_uri} ({(int)response.StatusCode} {response.ReasonPhrase})"
                );

            var raw = await response.Content.ReadAsStringAsync(ct);
            return new JsonConfigurationProvider(raw).Read();
        }
        catch (Exception ex) when (!ct.IsCancellationRequested && IsTimeout(ex))
        {
            throw new TimeoutException($"Json configuration fetch from {_uri} exceeded {_timeout}", ex);
        }
    }

    /// <summary>
    /// Recognises a <c>HttpClient.Timeout</c>-induced failure regardless of how the runtime
    /// wraps it (raw <see cref="TaskCanceledException"/>, or <see cref="HttpRequestException"/>
    /// with a <see cref="TimeoutException"/> inner).
    /// </summary>
    /// <param name="ex">Exception to inspect</param>
    /// <returns>True when the exception denotes a timeout</returns>
    private static bool IsTimeout(Exception ex) =>
        ex is TaskCanceledException
        || ex is TimeoutException
        || (ex is HttpRequestException && ex.InnerException is TimeoutException);
}
