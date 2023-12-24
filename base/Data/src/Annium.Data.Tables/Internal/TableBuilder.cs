using System;
using Annium.Logging;

namespace Annium.Data.Tables.Internal;

internal class TableBuilder<T> : ITableBuilder<T>
    where T : notnull
{
    private TablePermission _permissions;
    private GetKey<T>? _getKey;
    private HasChanged<T, T>? _hasChanged;
    private Update<T, T>? _update;
    private Func<T, bool>? _isActive;
    private readonly ILogger _logger;

    public TableBuilder(ILogger logger)
    {
        _logger = logger;
    }

    public ITableBuilder<T> Allow(TablePermission permissions)
    {
        _permissions = permissions;

        return this;
    }

    public ITableBuilder<T> Key(GetKey<T> getKey)
    {
        _getKey = getKey;

        return this;
    }

    public ITableBuilder<T> Set(HasChanged<T, T> hasChanged, Update<T, T> update)
    {
        _hasChanged = hasChanged;
        _update = update;

        return this;
    }

    public ITableBuilder<T> Keep(Func<T, bool> isActive)
    {
        _isActive = isActive;

        return this;
    }

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
