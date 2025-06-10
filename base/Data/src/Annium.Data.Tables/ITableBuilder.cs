using System;

namespace Annium.Data.Tables;

/// <summary>
/// Builder interface for configuring and creating tables
/// </summary>
/// <typeparam name="T">The type of items that will be stored in the table</typeparam>
public interface ITableBuilder<T>
    where T : notnull
{
    /// <summary>
    /// Configures the permissions allowed on the table
    /// </summary>
    /// <param name="permissions">The permissions to allow</param>
    /// <returns>The builder instance for method chaining</returns>
    ITableBuilder<T> Allow(TablePermission permissions);

    /// <summary>
    /// Configures the key extraction function for table items
    /// </summary>
    /// <param name="getKey">Function to extract keys from items</param>
    /// <returns>The builder instance for method chaining</returns>
    ITableBuilder<T> Key(GetKey<T> getKey);

    /// <summary>
    /// Configures the change detection and update behavior for table items
    /// </summary>
    /// <param name="hasChanged">Function to determine if an item has changed</param>
    /// <param name="update">Function to update an existing item</param>
    /// <returns>The builder instance for method chaining</returns>
    ITableBuilder<T> Set(HasChanged<T, T> hasChanged, Update<T, T> update);

    /// <summary>
    /// Configures the predicate to determine which items should remain active in the table
    /// </summary>
    /// <param name="isActive">Function to determine if an item should remain active</param>
    /// <returns>The builder instance for method chaining</returns>
    ITableBuilder<T> Keep(Func<T, bool> isActive);

    /// <summary>
    /// Builds the configured table instance
    /// </summary>
    /// <returns>A new table instance with the configured settings</returns>
    ITable<T> Build();
}
