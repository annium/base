using System.Collections;
using System.Collections.Generic;

namespace Annium.Testing;

public sealed class TestLog<T> : IReadOnlyList<T>
{
    private readonly List<T> _entries = new();

    public int Count
    {
        get
        {

            lock (_entries)
                return _entries.Count;
        }
    }

    public T this[int index]
    {
        get
        {
            lock (_entries)
                return _entries[index];
        }
    }

    public void Add(T message)
    {
        lock (_entries)
            _entries.Add(message);
    }

    public void Clear()
    {
        lock (_entries)
            _entries.Clear();
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (_entries)
            return _entries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (_entries)
            return _entries.GetEnumerator();
    }
}
