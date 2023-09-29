namespace Annium.Data.Tables;

public interface ITable<T> : ITableSource<T>, ITableView<T>
    where T : notnull
{
}