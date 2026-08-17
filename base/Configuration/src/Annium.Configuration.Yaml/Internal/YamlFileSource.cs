using System.Collections.Generic;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Yaml.Internal;

/// <summary>
/// Deferred configuration source that reads a YAML file at <see cref="FileConfigurationSourceBase.LoadAsync"/> time.
/// </summary>
internal sealed class YamlFileSource : FileConfigurationSourceBase
{
    /// <summary>
    /// Format label ("Yaml") used in diagnostic and error messages for this source.
    /// </summary>
    protected override string FormatLabel => "Yaml";

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlFileSource"/> class.
    /// </summary>
    /// <param name="path">Path of the YAML file to read.</param>
    /// <param name="optional">Whether a missing or unreadable file is silenced instead of thrown.</param>
    public YamlFileSource(string path, bool optional)
        : base(path, optional) { }

    /// <summary>
    /// Parses raw YAML text into the flattened configuration key/value dictionary.
    /// </summary>
    /// <param name="raw">Raw YAML document text loaded from the file.</param>
    /// <returns>Flattened configuration data keyed by path segments.</returns>
    protected override IReadOnlyDictionary<string[], string> ParseRaw(string raw) =>
        new YamlConfigurationProvider(raw).Read();
}
