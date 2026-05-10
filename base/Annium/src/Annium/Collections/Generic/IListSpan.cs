namespace Annium.Collections.Generic;

/// <summary>
/// Defines a span of elements from a read-only list with the ability to move the span's position.
/// </summary>
/// <remarks>
/// Implementations are typically <c>record</c> types so two spans with the same range compare equal
/// (relied on by range-comparison call sites such as <c>SortedListExtensions.GetChunks</c>). <b>Caveat:</b>
/// the compiler-synthesized <c>with</c>-expression bypasses the <see cref="Move(int)"/> bounds check, so
/// callers can construct an out-of-range span via <c>span with { Start = -1 }</c>. <see cref="Move(int)"/>
/// is the only sanctioned way to reposition the span and includes bounds checking.
/// </remarks>
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
