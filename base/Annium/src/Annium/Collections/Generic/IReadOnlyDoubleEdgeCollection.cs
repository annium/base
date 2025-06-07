using System.Collections.Generic;

namespace Annium.Collections.Generic;

/// <summary>
/// Defines a read-only collection with access to the first and last elements.
/// </summary>
/// <typeparam name="T">The type of the elements in the collection.</typeparam>
public interface IReadOnlyDoubleEdgeCollection<T> : IReadOnlyCollection<T>
{
    /// <summary>
    /// Gets the first element in the collection.
    /// </summary>
    T First { get; }

    /// <summary>
    /// Gets the last element in the collection.
    /// </summary>
    T Last { get; }
}
