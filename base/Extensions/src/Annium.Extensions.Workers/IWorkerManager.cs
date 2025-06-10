using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Workers;

/// <summary>
/// Interface for managing the lifecycle of keyed workers
/// </summary>
/// <typeparam name="TData">The type of key used to identify workers</typeparam>
public interface IWorkerManager<TData>
    where TData : IEquatable<TData>
{
    /// <summary>
    /// Starts a worker for the specified key
    /// </summary>
    /// <param name="key">The key identifying the worker to start</param>
    /// <returns>A task that completes when the worker is started</returns>
    Task StartAsync(TData key);

    /// <summary>
    /// Stops the worker for the specified key
    /// </summary>
    /// <param name="key">The key identifying the worker to stop</param>
    /// <returns>A task that completes when the worker is stopped</returns>
    Task StopAsync(TData key);
}
