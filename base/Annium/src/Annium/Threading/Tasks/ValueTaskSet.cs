using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annium.Threading.Tasks;

/// <summary>
/// Provides methods for working with sets of ValueTasks.
/// </summary>
public static class ValueTaskSet
{
#pragma warning disable VSTHRD200
    /// <summary>
    /// Waits for all ValueTasks to complete and returns their results as an array.
    /// </summary>
    /// <typeparam name="T">The type of the ValueTask's result.</typeparam>
    /// <param name="tasks">The list of ValueTasks to await.</param>
    /// <returns>An array containing the results of all ValueTasks.</returns>
    /// <exception cref="AggregateException">Thrown if one or more ValueTasks throw exceptions.</exception>
    public static async ValueTask<T[]> WhenAll<T>(IReadOnlyList<ValueTask<T>> tasks)
#pragma warning restore VSTHRD200
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

        return exceptions is null ? results : throw new AggregateException(exceptions);
    }

#pragma warning disable VSTHRD200
    /// <summary>
    /// Waits for all ValueTasks to complete.
    /// </summary>
    /// <param name="tasks">The list of ValueTasks to await.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    /// <exception cref="AggregateException">Thrown if one or more ValueTasks throw exceptions.</exception>
    public static async ValueTask WhenAll(IReadOnlyList<ValueTask> tasks)
#pragma warning restore VSTHRD200
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

#pragma warning disable VSTHRD200
    /// <summary>
    /// Waits for all ValueTasks to complete and returns their results as an array.
    /// </summary>
    /// <typeparam name="T">The type of the ValueTask's result.</typeparam>
    /// <param name="tasks">The enumerable of ValueTasks to await.</param>
    /// <returns>A ValueTask containing an array of results.</returns>
    public static ValueTask<T[]> WhenAll<T>(IEnumerable<ValueTask<T>> tasks) => WhenAll(tasks.ToList());

    /// <summary>
    /// Waits for all ValueTasks to complete.
    /// </summary>
    /// <param name="tasks">The enumerable of ValueTasks to await.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    public static ValueTask WhenAll(IEnumerable<ValueTask> tasks) => WhenAll(tasks.ToList());

    /// <summary>
    /// Waits for all ValueTasks to complete.
    /// </summary>
    /// <param name="tasks">The array of ValueTasks to await.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    public static ValueTask WhenAll(params ValueTask[] tasks) => WhenAll(tasks as IReadOnlyList<ValueTask>);
#pragma warning restore VSTHRD200
}
