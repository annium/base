using System;

namespace Annium.Data.Tables;

public interface ITableBuilder<T>
    where T : notnull
{
    ITableBuilder<T> Allow(TablePermission permissions);
    ITableBuilder<T> Key(GetKey<T> getKey);
    ITableBuilder<T> UpdateWith(HasChanged<T> hasChanged, Update<T> update);
    ITableBuilder<T> Keep(Func<T, bool> isActive);
    ITable<T> Build();
}
