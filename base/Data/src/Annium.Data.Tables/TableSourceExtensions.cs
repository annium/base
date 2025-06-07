using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Data.Tables;

/// <summary>
/// Extension methods for working with table sources and mapping operations.
/// </summary>
public static class TableSourceExtensions
{
    /// <summary>
    /// Maps and writes change events from a source observable to a target table source, replacing existing data on initialization.
    /// </summary>
    /// <typeparam name="TS">The source item type.</typeparam>
    /// <typeparam name="TD">The destination item type.</typeparam>
    /// <param name="source">The source observable of change events.</param>
    /// <param name="target">The target table source to write to.</param>
    /// <param name="map">The mapping function to transform source items to destination items.</param>
    /// <returns>A disposable subscription to the source observable.</returns>
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

    /// <summary>
    /// Maps and appends change events from a source observable to a target table source, adding to existing data on initialization.
    /// </summary>
    /// <typeparam name="TS">The source item type.</typeparam>
    /// <typeparam name="TD">The destination item type.</typeparam>
    /// <param name="source">The source observable of change events.</param>
    /// <param name="target">The target table source to append to.</param>
    /// <param name="map">The mapping function to transform source items to destination items.</param>
    /// <returns>A disposable subscription to the source observable.</returns>
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

    /// <summary>
    /// Maps a change event and writes it to the target table source, replacing existing data on initialization.
    /// </summary>
    /// <typeparam name="TS">The source item type.</typeparam>
    /// <typeparam name="TD">The destination item type.</typeparam>
    /// <param name="target">The target table source.</param>
    /// <param name="e">The change event to map and write.</param>
    /// <param name="map">The mapping function.</param>
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

    /// <summary>
    /// Maps a change event and appends it to the target table source, adding to existing data on initialization.
    /// </summary>
    /// <typeparam name="TS">The source item type.</typeparam>
    /// <typeparam name="TD">The destination item type.</typeparam>
    /// <param name="target">The target table source.</param>
    /// <param name="e">The change event to map and append.</param>
    /// <param name="map">The mapping function.</param>
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

    /// <summary>
    /// Synchronizes the target table source with the provided values by adding missing items and deleting unexpected ones.
    /// </summary>
    /// <typeparam name="T">The type of items in the table.</typeparam>
    /// <param name="target">The target table source to synchronize.</param>
    /// <param name="values">The collection of values to synchronize with.</param>
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

    /// <summary>
    /// Synchronizes the target table source with the provided values by adding missing items, updating existing ones, and deleting unexpected ones.
    /// </summary>
    /// <typeparam name="T">The type of items in the table.</typeparam>
    /// <param name="target">The target table source to synchronize.</param>
    /// <param name="values">The collection of values to synchronize with.</param>
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
