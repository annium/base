using System;
using Annium.Logging;

namespace Annium.Data.Tables.Internal;

/// <summary>
/// Internal implementation of a table builder for configuring and creating table instances.
/// </summary>
/// <typeparam name="T">The type of items that will be stored in the table.</typeparam>
internal class TableBuilder<T> : ITableBuilder<T>
    where T : notnull
{
    /// <summary>
    /// The permissions that will be granted to the table.
    /// </summary>
    private TablePermission _permissions;

    /// <summary>
    /// Function to extract keys from items.
    /// </summary>
    private GetKey<T>? _getKey;

    /// <summary>
    /// Function to determine if an item has changed.
    /// </summary>
    private HasChanged<T, T>? _hasChanged;

    /// <summary>
    /// Function to update an existing item with new data.
    /// </summary>
    private Update<T, T>? _update;

    /// <summary>
    /// Function to determine if an item should remain active in the table.
    /// </summary>
    private Func<T, bool>? _isActive;

    /// <summary>
    /// Logger instance for the table.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the TableBuilder class.
    /// </summary>
    /// <param name="logger">Logger instance for the table that will be built.</param>
    public TableBuilder(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Configures the permissions for the table being built.
    /// </summary>
    /// <param name="permissions">The permissions to grant to the table.</param>
    /// <returns>The table builder for method chaining.</returns>
    public ITableBuilder<T> Allow(TablePermission permissions)
    {
        _permissions = permissions;

        return this;
    }

    /// <summary>
    /// Configures the key extraction function for the table.
    /// </summary>
    /// <param name="getKey">Function to extract keys from items.</param>
    /// <returns>The table builder for method chaining.</returns>
    public ITableBuilder<T> Key(GetKey<T> getKey)
    {
        _getKey = getKey;

        return this;
    }

    /// <summary>
    /// Configures the change detection and update functions for the table.
    /// </summary>
    /// <param name="hasChanged">Function to determine if an item has changed.</param>
    /// <param name="update">Function to update an existing item with new data.</param>
    /// <returns>The table builder for method chaining.</returns>
    public ITableBuilder<T> Set(HasChanged<T, T> hasChanged, Update<T, T> update)
    {
        _hasChanged = hasChanged;
        _update = update;

        return this;
    }

    /// <summary>
    /// Configures the active item predicate for the table.
    /// </summary>
    /// <param name="isActive">Function to determine if an item should remain active in the table.</param>
    /// <returns>The table builder for method chaining.</returns>
    public ITableBuilder<T> Keep(Func<T, bool> isActive)
    {
        _isActive = isActive;

        return this;
    }

    /// <summary>
    /// Builds and returns a configured table instance.
    /// </summary>
    /// <returns>A new table instance with the configured settings.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing.</exception>
    public ITable<T> Build()
    {
        if (_getKey is null)
            throw new InvalidOperationException($"Table<{typeof(T).Name},{typeof(T).Name}> must have key");

        var getKey = _getKey ?? throw new InvalidOperationException("Key resolution function not specified");
        var hasChanged = _hasChanged ?? ((_, _) => true);
        var update = _update ?? throw new InvalidOperationException("Update function not specified");
        var isActive = _isActive ?? (_ => true);

        return new Table<T>(_permissions, getKey, hasChanged, update, isActive, _logger);
    }
}
