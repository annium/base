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
    /// <inheritdoc />
    protected override string FormatLabel => "Json";

    public JsonRemoteSource(Uri uri, bool optional, TimeSpan? timeout)
        : base(uri, optional, timeout) { }

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string[], string> ParseRaw(string raw) =>
        new JsonConfigurationProvider(raw).Read();
}
