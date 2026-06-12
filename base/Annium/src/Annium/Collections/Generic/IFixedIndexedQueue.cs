using System.Collections.Generic;

namespace Annium.Collections.Generic;

/// <summary>
/// Defines a fixed-size queue with indexed access to its elements.
/// </summary>
/// <typeparam name="T">The type of the elements in the queue.</typeparam>
public interface IFixedIndexedQueue<T> : IReadOnlyList<T>
{
    /// <summary>
    /// Gets the capacity of the queue.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Adds an item to the queue.
    /// </summary>
    /// <param name="item">The item to add.</param>
    void Add(T item);
}
