using System;

namespace Annium.Extensions.Workers;

/// <summary>
/// Extension methods for worker manager operations
/// </summary>
public static class WorkerManagerExtensions
{
    /// <summary>
    /// Sets the active state of a worker by starting or stopping it
    /// </summary>
    /// <typeparam name="TData">The type of key used to identify workers</typeparam>
    /// <param name="manager">The worker manager instance</param>
    /// <param name="key">The key identifying the worker</param>
    /// <param name="isActive">True to start the worker, false to stop it</param>
    public static void SetState<TData>(this IWorkerManager<TData> manager, TData key, bool isActive)
        where TData : IEquatable<TData>
    {
        if (isActive)
            manager.StartAsync(key).GetAwaiter();
        else
            manager.StopAsync(key).GetAwaiter();
    }

    /// <summary>
    /// Updates a worker's key and sets its active state, handling key transitions properly
    /// </summary>
    /// <typeparam name="TData">The type of key used to identify workers</typeparam>
    /// <param name="manager">The worker manager instance</param>
    /// <param name="oldKey">The previous key of the worker</param>
    /// <param name="newKey">The new key for the worker</param>
    /// <param name="isActive">True to start the worker with the new key, false to stop it</param>
    public static void SetStateWithKeyUpdate<TData>(
        this IWorkerManager<TData> manager,
        TData oldKey,
        TData newKey,
        bool isActive
    )
        where TData : IEquatable<TData>
    {
        if (newKey.Equals(oldKey))
            manager.SetState(newKey, isActive);
        else
        {
            manager.StopAsync(oldKey).GetAwaiter();
            if (isActive)
                manager.StartAsync(newKey).GetAwaiter();
        }
    }
}
