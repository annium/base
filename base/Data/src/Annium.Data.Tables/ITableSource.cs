using System.Collections.Generic;

namespace Annium.Data.Tables;

public interface ITableSource<T>
    where T : notnull
{
    int GetKey(T value);
    IReadOnlyDictionary<int, T> Source { get; }
    void Init(IReadOnlyCollection<T> entries);
    void Set(T entry);
    void Delete(T entry);
}