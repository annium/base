using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Base class for deferred configuration sources that read a local file at <see cref="LoadAsync"/> time.
/// </summary>
public abstract class FileConfigurationSourceBase : IConfigurationSource
{
    /// <summary>Absolute path to the configuration file.</summary>
    private readonly string _path;

    /// <summary>Whether a missing/unreadable file is silenced.</summary>
    public bool Optional { get; }

    /// <summary>Format label used in diagnostic messages (e.g. "Json", "Yaml").</summary>
    protected abstract string FormatLabel { get; }

    /// <summary>
    /// Parses the file contents into the flattened configuration dictionary.
    /// </summary>
    /// <param name="raw">Raw text read from the file.</param>
    /// <returns>Flattened configuration data.</returns>
    protected abstract IReadOnlyDictionary<string[], string> ParseRaw(string raw);

    /// <summary>Initializes a new instance of <see cref="FileConfigurationSourceBase"/>.</summary>
    /// <param name="path">Path to the configuration file (resolved to absolute).</param>
    /// <param name="optional">Whether a missing file is silenced.</param>
    protected FileConfigurationSourceBase(string path, bool optional)
    {
        _path = Path.GetFullPath(path);
        Optional = optional;
    }

    /// <summary>
    /// Reads the file and flattens it. Throws <see cref="FileNotFoundException"/> when the file
    /// is absent (caller decides whether to swallow via <see cref="IConfigurationSource.Optional"/>).
    /// </summary>
    /// <param name="ct">Cancellation token forwarded to the file read.</param>
    /// <returns>Flattened configuration.</returns>
    public async ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct)
    {
        if (!File.Exists(_path))
            throw new FileNotFoundException(
                $"{FormatLabel} configuration file {_path} not found and is not optional",
                _path
            );

        var raw = await File.ReadAllTextAsync(_path, ct);
        return ParseRaw(raw);
    }
}
