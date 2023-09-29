using System.Collections.Generic;

namespace Annium.Collections.Generic;

public interface IReadOnlyIndexedSpan<out T> : IReadOnlyList<T>
{
    int Start { get; }
    int End { get; }
}