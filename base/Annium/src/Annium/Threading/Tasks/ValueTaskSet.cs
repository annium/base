using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annium.Threading.Tasks;

public static class ValueTaskSet
{
    public static async ValueTask<T[]> WhenAll<T>(IReadOnlyList<ValueTask<T>> tasks)
    {
        List<Exception>? exceptions = null;

        var results = new T[tasks.Count];
        for (var i = 0; i < tasks.Count; i++)
            try
            {
                results[i] = await tasks[i].ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>(tasks.Count);
                exceptions.Add(ex);
            }

        return exceptions is null
            ? results
            : throw new AggregateException(exceptions);
    }

    public static async ValueTask WhenAll(IReadOnlyList<ValueTask> tasks)
    {
        List<Exception>? exceptions = null;

        foreach (var task in tasks)
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>(tasks.Count);
                exceptions.Add(ex);
            }

        if (exceptions is not null)
            throw new AggregateException(exceptions);
    }

    public static ValueTask<T[]> WhenAll<T>(IEnumerable<ValueTask<T>> tasks) =>
        WhenAll(tasks.ToList());

    public static ValueTask WhenAll(IEnumerable<ValueTask> tasks) =>
        WhenAll(tasks.ToList());

    public static ValueTask WhenAll(params ValueTask[] tasks) =>
        WhenAll(tasks as IReadOnlyList<ValueTask>);
}