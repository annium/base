using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Workers;

/// <summary>
/// Base class for implementing workers that can be managed by the worker manager
/// </summary>
/// <typeparam name="TKey">The type of key used to identify this worker</typeparam>
public abstract class WorkerBase<TKey> : IAsyncDisposable
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets the key that identifies this worker instance
    /// </summary>
    protected TKey Key { get; private set; } = default!;

    /// <summary>
    /// Cancellation token source for managing worker lifecycle cancellation
    /// </summary>
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Initializes the worker with the specified key and starts it
    /// </summary>
    /// <param name="key">The key to identify this worker instance</param>
    /// <returns>A task that completes when initialization is finished</returns>
    public ValueTask InitAsync(TKey key)
    {
        Key = key;
        return StartAsync(_cts.Token);
    }

    /// <summary>
    /// Starts the worker with the provided cancellation token
    /// </summary>
    /// <param name="сt">Cancellation token for stopping the worker</param>
    /// <returns>A task that completes when the worker has started</returns>
    protected abstract ValueTask StartAsync(CancellationToken сt);

    /// <summary>
    /// Stops the worker and cleans up any resources
    /// </summary>
    /// <returns>A task that completes when the worker has stopped</returns>
    protected abstract ValueTask StopAsync();

    /// <summary>
    /// Disposes the worker by cancelling operations and stopping
    /// </summary>
    /// <returns>A task that completes when disposal is finished</returns>
    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        await StopAsync();
    }
}
