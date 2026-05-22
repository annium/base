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
    /// <inheritdoc />
    protected override string FormatLabel => "Yaml";

    public YamlRemoteSource(Uri uri, bool optional, TimeSpan? timeout)
        : base(uri, optional, timeout) { }

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string[], string> ParseRaw(string raw) =>
        new YamlConfigurationProvider(raw).Read();
}
