using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Yaml.Internal;

/// <summary>
/// Deferred configuration source that reads a YAML file at <see cref="LoadAsync"/> time.
/// </summary>
internal sealed class YamlFileSource : IConfigurationSource
{
    /// <summary>Absolute path to the YAML file.</summary>
    private readonly string _path;

    /// <summary>Whether a missing/unreadable file is silenced.</summary>
    public bool Optional { get; }

    public YamlFileSource(string path, bool optional)
    {
        _path = Path.GetFullPath(path);
        Optional = optional;
    }

    /// <summary>
    /// Reads the YAML file and flattens it. Throws <see cref="FileNotFoundException"/> when the
    /// file is absent (caller decides whether to swallow via <see cref="IConfigurationSource.Optional"/>).
    /// </summary>
    /// <param name="ct">Cancellation token forwarded to the file read.</param>
    /// <returns>Flattened YAML configuration.</returns>
    public async ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct)
    {
        if (!File.Exists(_path))
            throw new FileNotFoundException($"Yaml configuration file {_path} not found and is not optional", _path);

        var raw = await File.ReadAllTextAsync(_path, ct);
        return new YamlConfigurationProvider(raw).Read();
    }
}
