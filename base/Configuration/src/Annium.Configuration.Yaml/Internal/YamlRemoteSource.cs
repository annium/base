using System;
using System.Collections.Generic;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Yaml.Internal;

/// <summary>
/// Deferred configuration source that fetches a YAML document from a remote endpoint at
/// <see cref="RemoteConfigurationSourceBase.LoadAsync"/> time.
/// </summary>
internal sealed class YamlRemoteSource : RemoteConfigurationSourceBase
{
    /// <summary>
    /// Format label ("Yaml") used in diagnostic and error messages for this source.
    /// </summary>
    protected override string FormatLabel => "Yaml";

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlRemoteSource"/> class.
    /// </summary>
    /// <param name="uri">Address the YAML document is fetched from.</param>
    /// <param name="optional">Whether a fetch failure is silenced instead of thrown.</param>
    /// <param name="timeout">Request timeout, or <c>null</c> to use the default.</param>
    public YamlRemoteSource(Uri uri, bool optional, TimeSpan? timeout)
        : base(uri, optional, timeout) { }

    /// <summary>
    /// Parses raw YAML text into the flattened configuration key/value dictionary.
    /// </summary>
    /// <param name="raw">Raw YAML document text fetched from the remote endpoint.</param>
    /// <returns>Flattened configuration data keyed by path segments.</returns>
    protected override IReadOnlyDictionary<string[], string> ParseRaw(string raw) =>
        new YamlConfigurationProvider(raw).Read();
}
