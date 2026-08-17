using System.Collections.Generic;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Json.Internal;

/// <summary>
/// Deferred configuration source that reads a JSON file at <see cref="FileConfigurationSourceBase.LoadAsync"/> time.
/// </summary>
internal sealed class JsonFileSource : FileConfigurationSourceBase
{
    /// <summary>
    /// Format label ("Json") used in diagnostic and error messages for this source.
    /// </summary>
    protected override string FormatLabel => "Json";

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileSource"/> class.
    /// </summary>
    /// <param name="path">Path of the JSON file to read.</param>
    /// <param name="optional">Whether a missing or unreadable file is silenced instead of thrown.</param>
    public JsonFileSource(string path, bool optional)
        : base(path, optional) { }

    /// <summary>
    /// Parses raw JSON text into the flattened configuration key/value dictionary.
    /// </summary>
    /// <param name="raw">Raw JSON document text loaded from the file.</param>
    /// <returns>Flattened configuration data keyed by path segments.</returns>
    protected override IReadOnlyDictionary<string[], string> ParseRaw(string raw) =>
        new JsonConfigurationProvider(raw).Read();
}
