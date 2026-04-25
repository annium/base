using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Json.Internal;

/// <summary>
/// Deferred configuration source that reads a JSON file at <see cref="LoadAsync"/> time.
/// </summary>
internal sealed class JsonFileSource : IConfigurationSource
{
    /// <summary>Absolute path to the JSON file.</summary>
    private readonly string _path;

    /// <summary>Whether a missing/unreadable file is silenced.</summary>
    public bool Optional { get; }

    public JsonFileSource(string path, bool optional)
    {
        _path = Path.GetFullPath(path);
        Optional = optional;
    }

    /// <summary>
    /// Reads the JSON file and flattens it. Throws <see cref="FileNotFoundException"/> when the
    /// file is absent (caller decides whether to swallow via <see cref="IConfigurationSource.Optional"/>).
    /// </summary>
    /// <param name="ct">Cancellation token forwarded to the file read.</param>
    /// <returns>Flattened JSON configuration.</returns>
    public async ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct)
    {
        if (!File.Exists(_path))
            throw new FileNotFoundException($"Json configuration file {_path} not found and is not optional", _path);

        var raw = await File.ReadAllTextAsync(_path, ct);
        return new JsonConfigurationProvider(raw).Read();
    }
}
