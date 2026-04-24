using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Workers;

/// <summary>
/// Extension methods for worker manager operations
/// </summary>
public static class WorkerManagerExtensions
{
    /// <summary>
    /// Sets the active state of a worker by starting or stopping it.
    /// </summary>
    /// <typeparam name="TData">The type of key used to identify workers</typeparam>
    /// <param name="manager">The worker manager instance</param>
    /// <param name="key">The key identifying the worker</param>
    /// <param name="isActive">True to start the worker, false to stop it</param>
    /// <returns>A task that completes when the underlying start/stop operation finishes.</returns>
    public static Task SetStateAsync<TData>(this IWorkerManager<TData> manager, TData key, bool isActive)
        where TData : IEquatable<TData>
    {
        return isActive ? manager.StartAsync(key) : manager.StopAsync(key);
    }

    /// <summary>
    /// Updates a worker's key and sets its active state, handling key transitions properly.
    /// </summary>
    /// <typeparam name="TData">The type of key used to identify workers</typeparam>
    /// <param name="manager">The worker manager instance</param>
    /// <param name="oldKey">The previous key of the worker</param>
    /// <param name="newKey">The new key for the worker</param>
    /// <param name="isActive">True to start the worker with the new key, false to stop it</param>
    /// <returns>A task that completes when the underlying transition operations finish.</returns>
    public static async Task SetStateWithKeyUpdateAsync<TData>(
        this IWorkerManager<TData> manager,
        TData oldKey,
        TData newKey,
        bool isActive
    )
        where TData : IEquatable<TData>
    {
        if (newKey.Equals(oldKey))
        {
            await manager.SetStateAsync(newKey, isActive);
            return;
        }

        await manager.StopAsync(oldKey);
        if (isActive)
            await manager.StartAsync(newKey);
    }
}
