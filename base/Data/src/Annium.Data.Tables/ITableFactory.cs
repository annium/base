namespace Annium.Data.Tables;

/// <summary>
/// Factory interface for creating table builders
/// </summary>
public interface ITableFactory
{
    /// <summary>
    /// Creates a new table builder for the specified type
    /// </summary>
    /// <typeparam name="T">The type of items that will be stored in the table</typeparam>
    /// <returns>A new table builder instance</returns>
    ITableBuilder<T> New<T>()
        where T : notnull;
}
