using System.Collections;
using System.Collections.Generic;

namespace Annium.Testing;

/// <summary>
/// Represents a thread-safe log for test messages of a specified type.
/// </summary>
/// <typeparam name="T">The type of log entries.</typeparam>
public sealed class TestLog<T> : IReadOnlyList<T>
{
    /// <summary>
    /// The list of log entries.
    /// </summary>
    private readonly List<T> _entries = new();

    /// <summary>
    /// Gets the number of log entries.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_entries)
                return _entries.Count;
        }
    }

    /// <summary>
    /// Gets the log entry at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the entry to get.</param>
    /// <returns>The log entry at the specified index.</returns>
    public T this[int index]
    {
        get
        {
            lock (_entries)
                return _entries[index];
        }
    }

    /// <summary>
    /// Adds a message to the log.
    /// </summary>
    /// <param name="message">The message to add.</param>
    public void Add(T message)
    {
        lock (_entries)
            _entries.Add(message);
    }

    /// <summary>
    /// Clears all log entries.
    /// </summary>
    public void Clear()
    {
        lock (_entries)
            _entries.Clear();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the log entries.
    /// </summary>
    /// <returns>An enumerator for the log entries.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        lock (_entries)
            return _entries.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the log entries.
    /// </summary>
    /// <returns>An enumerator for the log entries.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (_entries)
            return _entries.GetEnumerator();
    }
}
