using System.Collections.Generic;

namespace Annium.Data.Tables;

/// <summary>
/// Interface for the source capabilities of a table
/// </summary>
/// <typeparam name="T">The type of items stored in the table</typeparam>
public interface ITableSource<T>
    where T : notnull
{
    /// <summary>
    /// Gets the key for the specified value
    /// </summary>
    /// <param name="value">The value to get the key for</param>
    /// <returns>The key for the value</returns>
    int GetKey(T value);

    /// <summary>
    /// Gets the source dictionary containing all items keyed by their keys
    /// </summary>
    IReadOnlyDictionary<int, T> Source { get; }

    /// <summary>
    /// Initializes the table with a collection of entries
    /// </summary>
    /// <param name="entries">The entries to initialize the table with</param>
    void Init(IReadOnlyCollection<T> entries);

    /// <summary>
    /// Sets (adds or updates) an entry in the table
    /// </summary>
    /// <param name="entry">The entry to set</param>
    void Set(T entry);

    /// <summary>
    /// Deletes an entry from the table
    /// </summary>
    /// <param name="entry">The entry to delete</param>
    void Delete(T entry);
}
