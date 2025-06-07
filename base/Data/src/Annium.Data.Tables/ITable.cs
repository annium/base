namespace Annium.Data.Tables;

/// <summary>
/// Represents a complete table interface that combines source and view capabilities
/// </summary>
/// <typeparam name="T">The type of items stored in the table</typeparam>
public interface ITable<T> : ITableSource<T>, ITableView<T>
    where T : notnull;
