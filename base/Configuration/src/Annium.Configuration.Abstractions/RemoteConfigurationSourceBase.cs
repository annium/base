using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Base class for deferred configuration sources that fetch a document from a remote endpoint at
/// <see cref="LoadAsync"/> time. Honors a per-source timeout via a per-call linked
/// <see cref="CancellationTokenSource"/> (<see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/>);
/// the shared client runs with <see cref="Timeout.InfiniteTimeSpan"/>.
/// </summary>
public abstract class RemoteConfigurationSourceBase : IConfigurationSource
{
    /// <summary>Default timeout when none is supplied at registration.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Shared client for every remote source. Its own <see cref="HttpClient.Timeout"/> is disabled
    /// (infinite); the per-source timeout is enforced via a linked <see cref="CancellationTokenSource"/>
    /// in <see cref="LoadAsync"/>. A single client avoids the socket exhaustion of per-call instances.
    /// </summary>
    private static readonly HttpClient _client = new() { Timeout = Timeout.InfiniteTimeSpan };

    /// <summary>The URI to fetch.</summary>
    private readonly Uri _uri;

    /// <summary>Maximum time before the HTTP call is aborted.</summary>
    private readonly TimeSpan _timeout;

    /// <summary>Whether a fetch failure (non-2xx, network error, timeout) is silenced.</summary>
    public bool Optional { get; }

    /// <summary>Format label used in diagnostic messages (e.g. "Json", "Yaml").</summary>
    protected abstract string FormatLabel { get; }

    /// <summary>
    /// Parses the fetched payload into the flattened configuration dictionary.
    /// </summary>
    /// <param name="raw">Raw payload returned by the remote endpoint.</param>
    /// <returns>Flattened configuration data.</returns>
    protected abstract IReadOnlyDictionary<string[], string> ParseRaw(string raw);

    /// <summary>Initializes a new instance of <see cref="RemoteConfigurationSourceBase"/>.</summary>
    /// <param name="uri">URI to fetch the configuration from.</param>
    /// <param name="optional">Whether fetch failures are silenced.</param>
    /// <param name="timeout">Per-source timeout; defaults to <see cref="DefaultTimeout"/> when null.</param>
    protected RemoteConfigurationSourceBase(Uri uri, bool optional, TimeSpan? timeout)
    {
        _uri = uri;
        _timeout = timeout ?? DefaultTimeout;
        Optional = optional;
    }

    /// <summary>
    /// Issues a GET to the configured URI and flattens the response. Translates
    /// <see cref="TaskCanceledException"/> to <see cref="TimeoutException"/> when the per-source
    /// timeout is the cancellation cause, so the caller can distinguish.
    /// </summary>
    /// <param name="ct">Cancellation token forwarded by <c>BuildAsync</c>.</param>
    /// <returns>Flattened configuration.</returns>
    public async ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);
        try
        {
            using var response = await _client.GetAsync(_uri, cts.Token);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"{FormatLabel} configuration not available at {_uri} ({(int)response.StatusCode} {response.ReasonPhrase})"
                );

            var raw = await response.Content.ReadAsStringAsync(cts.Token);
            return ParseRaw(raw);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested && IsTimeout(ex))
        {
            throw new TimeoutException($"{FormatLabel} configuration fetch from {_uri} exceeded {_timeout}", ex);
        }
    }

    /// <summary>
    /// Recognises a timeout-induced cancellation. The caller-token guard on the catch filter has
    /// already excluded caller cancellation, so the only cancellation that can reach here is the
    /// per-source <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/> firing.
    /// </summary>
    /// <param name="ex">Exception to inspect</param>
    /// <returns>True when the exception denotes a timeout</returns>
    private static bool IsTimeout(Exception ex) => ex is OperationCanceledException;
}
