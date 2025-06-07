using System.Collections.Generic;

namespace Annium.Collections.Generic;

/// <summary>
/// Defines a read-only span of elements with a start and end index.
/// </summary>
/// <typeparam name="T">The type of the elements in the span.</typeparam>
public interface IReadOnlyIndexedSpan<out T> : IReadOnlyList<T>
{
    /// <summary>
    /// Gets the start index of the span.
    /// </summary>
    int Start { get; }

    /// <summary>
    /// Gets the end index of the span.
    /// </summary>
    int End { get; }
}
