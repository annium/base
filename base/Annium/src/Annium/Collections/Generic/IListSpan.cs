namespace Annium.Collections.Generic;

/// <summary>
/// Defines a span of elements from a read-only list with the ability to move the span's position.
/// </summary>
/// <typeparam name="T">The type of the elements in the span.</typeparam>
public interface IListSpan<out T> : IReadOnlyIndexedSpan<T>
{
    /// <summary>
    /// Moves the span by the specified offset.
    /// </summary>
    /// <param name="offset">The number of positions to move the span.</param>
    /// <returns>True if the move was successful; otherwise, false.</returns>
    bool Move(int offset);
}
