namespace Annium.Collections.Generic;

/// <summary>
/// Defines a double-ended queue (deque) with operations to add and remove elements from both ends.
/// </summary>
/// <typeparam name="T">The type of the elements in the queue.</typeparam>
public interface IDoubleEdgeQueue<T> : IReadOnlyDoubleEdgeCollection<T>
{
    /// <summary>
    /// Adds an element to the front of the queue.
    /// </summary>
    /// <param name="value">The element to add.</param>
    void AddFirst(T value);

    /// <summary>
    /// Adds an element to the end of the queue.
    /// </summary>
    /// <param name="value">The element to add.</param>
    void AddLast(T value);

    /// <summary>
    /// Removes the first element from the queue.
    /// </summary>
    void RemoveFirst();

    /// <summary>
    /// Removes the last element from the queue.
    /// </summary>
    void RemoveLast();
}
