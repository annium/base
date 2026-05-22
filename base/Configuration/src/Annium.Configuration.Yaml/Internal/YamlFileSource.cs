using System.Collections.Generic;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Yaml.Internal;

/// <summary>
/// Deferred configuration source that reads a YAML file at <see cref="FileConfigurationSourceBase.LoadAsync"/> time.
/// </summary>
internal sealed class YamlFileSource : FileConfigurationSourceBase
{
    /// <inheritdoc />
    protected override string FormatLabel => "Yaml";

    public YamlFileSource(string path, bool optional)
        : base(path, optional) { }

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string[], string> ParseRaw(string raw) =>
        new YamlConfigurationProvider(raw).Read();
}
