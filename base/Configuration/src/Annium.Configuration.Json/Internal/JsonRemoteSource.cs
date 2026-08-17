using System;
using System.Collections.Generic;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Json.Internal;

/// <summary>
/// Deferred configuration source that fetches a JSON document from a remote endpoint at
/// <see cref="RemoteConfigurationSourceBase.LoadAsync"/> time.
/// </summary>
internal sealed class JsonRemoteSource : RemoteConfigurationSourceBase
{
    /// <summary>
    /// Format label ("Json") used in diagnostic and error messages for this source.
    /// </summary>
    protected override string FormatLabel => "Json";

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRemoteSource"/> class.
    /// </summary>
    /// <param name="uri">Address the JSON document is fetched from.</param>
    /// <param name="optional">Whether a fetch failure is silenced instead of thrown.</param>
    /// <param name="timeout">Request timeout, or <c>null</c> to use the default.</param>
    public JsonRemoteSource(Uri uri, bool optional, TimeSpan? timeout)
        : base(uri, optional, timeout) { }

    /// <summary>
    /// Parses raw JSON text into the flattened configuration key/value dictionary.
    /// </summary>
    /// <param name="raw">Raw JSON document text fetched from the remote endpoint.</param>
    /// <returns>Flattened configuration data keyed by path segments.</returns>
    protected override IReadOnlyDictionary<string[], string> ParseRaw(string raw) =>
        new JsonConfigurationProvider(raw).Read();
}
