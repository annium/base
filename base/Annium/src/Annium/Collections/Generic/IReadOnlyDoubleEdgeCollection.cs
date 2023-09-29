using System.Collections.Generic;

namespace Annium.Collections.Generic;

public interface IReadOnlyDoubleEdgeCollection<T> : IReadOnlyCollection<T>
{
    T First { get; }
    T Last { get; }
}