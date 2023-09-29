using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Data.Tables;

public static class TableSourceExtensions
{
    public static IDisposable MapWriteTo<TS, TD>(
        this IObservable<IChangeEvent<TS>> source,
        ITableSource<TD> target,
        Func<TS, TD?> map
    )
        where TD : notnull
        => source.Subscribe(e => target.MapWrite(e, map));

    public static IDisposable MapAppendTo<TS, TD>(
        this IObservable<IChangeEvent<TS>> source,
        ITableSource<TD> target,
        Func<TS, TD?> map
    )
        where TD : notnull
        => source.Subscribe(e => target.MapAppend(e, map));

    private static void MapWrite<TS, TD>(
        this ITableSource<TD> target,
        IChangeEvent<TS> e,
        Func<TS, TD?> map
    )
        where TD : notnull
    {
        switch (e)
        {
            case InitEvent<TS> init:
                target.Init(init.Values.Select(map).OfType<TD>().ToArray());
                break;
            case AddEvent<TS> add:
                var addValue = map(add.Value);
                if (addValue is not null)
                    target.Set(addValue);
                break;
            case UpdateEvent<TS> update:
                var updateValue = map(update.Value);
                if (updateValue is not null)
                    target.Set(updateValue);
                break;
            case DeleteEvent<TS> delete:
                var deleteValue = map(delete.Value);
                if (deleteValue is not null)
                    target.Delete(deleteValue);
                break;
        }
    }

    private static void MapAppend<TS, TD>(
        this ITableSource<TD> target,
        IChangeEvent<TS> e,
        Func<TS, TD?> map
    )
        where TD : notnull
    {
        switch (e)
        {
            case InitEvent<TS> init:
                foreach (var value in init.Values)
                {
                    var initValue = map(value);
                    if (initValue is not null)
                        target.Set(initValue);
                }

                break;
            case AddEvent<TS> add:
                var addValue = map(add.Value);
                if (addValue is not null)
                    target.Set(addValue);
                break;
            case UpdateEvent<TS> update:
                var updateValue = map(update.Value);
                if (updateValue is not null)
                    target.Set(updateValue);
                break;
            case DeleteEvent<TS> delete:
                var deleteValue = map(delete.Value);
                if (deleteValue is not null)
                    target.Delete(deleteValue);
                break;
        }
    }

    public static void SyncAddDelete<T>(
        this ITableSource<T> target,
        IReadOnlyCollection<T> values
    )
        where T : notnull
    {
        var source = target.Source;
        var data = values.ToDictionary(target.GetKey, x => x);

        // remove unexpected values
        foreach (var (key, value) in source)
            if (!data.ContainsKey(key))
                target.Delete(value);

        // add missing values
        foreach (var (key, value) in data)
            if (!source.ContainsKey(key))
                target.Set(value);
    }

    public static void SyncAddUpdateDelete<T>(
        this ITableSource<T> target,
        IReadOnlyCollection<T> values
    )
        where T : notnull
    {
        var source = target.Source;
        var data = values.ToDictionary(target.GetKey, x => x);

        // remove unexpected values
        foreach (var (key, value) in source)
            if (!data.ContainsKey(key))
                target.Delete(value);

        // add or update values
        foreach (var value in values)
            target.Set(value);
    }
}