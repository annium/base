using System;
using System.Linq.Expressions;
using Annium.Logging;

namespace Annium.Data.Tables.Internal;

internal class TableBuilder<T> : ITableBuilder<T>
    where T : notnull
{
    private TablePermission _permissions;
    private Expression<Func<T, object>>? _getKey;
    private HasChanged<T>? _hasChanged;
    private Update<T>? _update;
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

    public ITableBuilder<T> Key(Expression<Func<T, object>> getKey)
    {
        _getKey = getKey;

        return this;
    }

    public ITableBuilder<T> UpdateWith(HasChanged<T> hasChanged, Update<T> update)
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

        var getKey = TableHelper.BuildGetKey(_getKey);
        var hasChanged = _hasChanged ?? ((_, _) => true);
        var update = _update ?? TableHelper.BuildUpdate<T>(_permissions);
        var isActive = _isActive ?? (_ => true);

        return new Table<T>(_permissions, getKey, hasChanged, update, isActive, _logger);
    }
}