using System.Collections.Generic;
using System.Linq;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Base class for configuration providers that read configuration data from various sources.
/// Subclasses produce flattened key-value data by entering/leaving named scopes with
/// <see cref="Push"/> / <see cref="Pop"/> and writing leaf values with <see cref="Set"/>.
/// </summary>
public abstract class ConfigurationProviderBase
{
    /// <summary>
    /// Flattened key-value store. Owned by the base — never reassigned, only cleared on <see cref="Init"/>.
    /// </summary>
    private readonly Dictionary<string[], string> _data = new();

    /// <summary>
    /// Context stack used to build the current key path. Owned by the base — never reassigned, only cleared on <see cref="Init"/>.
    /// </summary>
    private readonly Stack<string> _context = new();

    /// <summary>
    /// Gets the current path as an array of strings from the context stack.
    /// </summary>
    protected string[] Path => _context.Reverse().ToArray();

    /// <summary>
    /// Snapshot of the accumulated data, suitable for returning from <see cref="Read"/>.
    /// Returns a fresh dictionary copy each access — the caller's reference is not aliased to
    /// the live <c>_data</c> field, so a subsequent <see cref="Read"/> cannot mutate it.
    /// </summary>
    protected IReadOnlyDictionary<string[], string> Result => new Dictionary<string[], string>(_data);

    /// <summary>
    /// Reads configuration data from the source and returns it as a dictionary.
    /// </summary>
    /// <returns>Dictionary containing configuration keys and values</returns>
    public abstract IReadOnlyDictionary<string[], string> Read();

    /// <summary>
    /// Resets the provider state so the same instance can be re-read deterministically.
    /// Subclasses call this at the top of <see cref="Read"/>.
    /// </summary>
    protected void Init()
    {
        _data.Clear();
        _context.Clear();
    }

    /// <summary>
    /// Pushes a new segment onto the current context path.
    /// </summary>
    /// <param name="segment">Path segment to push.</param>
    protected void Push(string segment) => _context.Push(segment);

    /// <summary>
    /// Pops the most recently pushed segment off the context path.
    /// </summary>
    protected void Pop() => _context.Pop();

    /// <summary>
    /// Writes a leaf value at the current <see cref="Path"/>.
    /// </summary>
    /// <param name="value">Value to write.</param>
    protected void Set(string value) => _data[Path] = value;

    /// <summary>
    /// Writes a leaf value at an explicit path. Use when the subclass builds its own
    /// key path instead of accumulating it through <see cref="Push"/> / <see cref="Pop"/>.
    /// </summary>
    /// <param name="path">Explicit key path.</param>
    /// <param name="value">Value to write.</param>
    protected void SetAt(string[] path, string value) => _data[path] = value;
}
