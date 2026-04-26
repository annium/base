using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Configuration.Abstractions.Internal;

/// <summary>
/// Configuration source that flattens an in-memory object into path-segmented key/value pairs.
/// All work is synchronous; <c>LoadAsync</c> returns a completed <see cref="ValueTask"/>.
/// </summary>
internal sealed class ObjectConfigurationSource : IConfigurationSource
{
    /// <summary>The object to flatten.</summary>
    private readonly object? _config;

    /// <summary>Whether a flatten failure is silenced.</summary>
    public bool Optional { get; }

    public ObjectConfigurationSource(object? config, bool optional)
    {
        _config = config;
        Optional = optional;
    }

    /// <summary>
    /// Flattens the object into a path-segmented dictionary. Synchronous in nature — returned
    /// as a completed <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="ct">Cancellation token (unused for synchronous flatten).</param>
    /// <returns>Flattened configuration dictionary.</returns>
    public ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct) =>
        new(new ObjectConfigurationProvider(_config).Read());
}
