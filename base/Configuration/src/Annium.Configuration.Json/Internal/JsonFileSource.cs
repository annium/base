using System.Collections.Generic;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Json.Internal;

/// <summary>
/// Deferred configuration source that reads a JSON file at <see cref="FileConfigurationSourceBase.LoadAsync"/> time.
/// </summary>
internal sealed class JsonFileSource : FileConfigurationSourceBase
{
    /// <inheritdoc />
    protected override string FormatLabel => "Json";

    public JsonFileSource(string path, bool optional)
        : base(path, optional) { }

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string[], string> ParseRaw(string raw) =>
        new JsonConfigurationProvider(raw).Read();
}
