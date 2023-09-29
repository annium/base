namespace Annium.Collections.Generic;

public interface IDoubleEdgeQueue<T> : IReadOnlyDoubleEdgeCollection<T>
{
    void AddFirst(T value);
    void AddLast(T value);
    void RemoveFirst();
    void RemoveLast();
}