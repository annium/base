using System;

namespace Annium.Data.Tables;

public interface ITableBuilder<T>
    where T : notnull
{
    ITableBuilder<T> Allow(TablePermission permissions);
    ITableBuilder<T> Key(GetKey<T> getKey);
    ITableBuilder<T> Set(HasChanged<T, T> hasChanged, Update<T, T> update);
    ITableBuilder<T> Keep(Func<T, bool> isActive);
    ITable<T> Build();
}
