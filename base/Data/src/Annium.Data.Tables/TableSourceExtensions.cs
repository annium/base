using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Data.Tables;

public static class TableSourceExtensions
{
    public static IDisposable MapWriteTo<TS, TD>(
        this IObservable<ChangeEvent<TS>> source,
        ITableSource<TD> target,
        Func<TS, TD?> map
    )
        where TS : notnull
        where TD : notnull
    {
        return source.Subscribe(e => target.MapWrite(e, map));
    }

    public static IDisposable MapAppendTo<TS, TD>(
        this IObservable<ChangeEvent<TS>> source,
        ITableSource<TD> target,
        Func<TS, TD?> map
    )
        where TS : notnull
        where TD : notnull
    {
        return source.Subscribe(e => target.MapAppend(e, map));
    }

    private static void MapWrite<TS, TD>(this ITableSource<TD> target, ChangeEvent<TS> e, Func<TS, TD?> map)
        where TS : notnull
        where TD : notnull
    {
        switch (e.Type)
        {
            case ChangeEventType.Init:
                target.Init(e.Items.Select(map).OfType<TD>().ToArray());
                break;
            case ChangeEventType.Set:
                {
                    var item = map(e.Item);
                    if (item is not null)
                        target.Set(item);
                }
                break;
            case ChangeEventType.Delete:
                {
                    var item = map(e.Item);
                    if (item is not null)
                        target.Delete(item);
                }
                break;
        }
    }

    private static void MapAppend<TS, TD>(this ITableSource<TD> target, ChangeEvent<TS> e, Func<TS, TD?> map)
        where TS : notnull
        where TD : notnull
    {
        switch (e.Type)
        {
            case ChangeEventType.Init:
                foreach (var i in e.Items)
                {
                    var item = map(i);
                    if (item is not null)
                        target.Set(item);
                }

                break;
            case ChangeEventType.Set:
                {
                    var item = map(e.Item);
                    if (item is not null)
                        target.Set(item);
                }
                break;
            case ChangeEventType.Delete:
                {
                    var item = map(e.Item);
                    if (item is not null)
                        target.Delete(item);
                }
                break;
        }
    }

    public static void SyncAddDelete<T>(this ITableSource<T> target, IReadOnlyCollection<T> values)
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

    public static void SyncAddUpdateDelete<T>(this ITableSource<T> target, IReadOnlyCollection<T> values)
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
