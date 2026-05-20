using System;

namespace Annium.Internal.Collections.Generic;

/// <summary>
/// Centralised formatters for the messages used by every fixed-size span/queue type in
/// <see cref="Annium.Collections.Generic"/> (ListSpan, SortedListSpan, FixedIndexedQueue, ...).
/// Keeps the message text in one place so a wording change touches one site.
/// </summary>
internal static class IndexOutOfRangeMessage
{
    /// <summary>
    /// Builds the standard "index out of range [0;count-1]" message.
    /// </summary>
    /// <param name="index">The offending index value.</param>
    /// <param name="count">The collection's current count (used to compute the upper bound).</param>
    /// <returns>The formatted message.</returns>
    public static string For(int index, int count) => $"Index {index} is out of range [0;{count - 1}]";

    /// <summary>
    /// Validates a span's <c>(start, count)</c> against the underlying collection size, throwing the
    /// standard "Invalid span" message on failure. Centralises the byte-identical validation used by
    /// <c>ListSpan</c> and <c>SortedListSpan</c>.
    /// </summary>
    /// <param name="start">The starting index of the span.</param>
    /// <param name="count">The number of elements in the span.</param>
    /// <param name="collectionCount">The size of the underlying collection.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> is negative or the span overruns the collection.</exception>
    public static void ValidateSpanRange(int start, int count, int collectionCount)
    {
        if (start < 0 || start + count > collectionCount)
            throw new ArgumentOutOfRangeException(
                nameof(start),
                $"Invalid span at {start} with length {count} for collection of size {collectionCount}"
            );
    }
}
