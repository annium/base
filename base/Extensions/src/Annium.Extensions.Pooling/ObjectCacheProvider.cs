using System.Threading;
using System.Threading.Tasks;
using OneOf;

namespace Annium.Extensions.Pooling;

/// <summary>
/// Abstract base class for providing object lifecycle management in object caches.
/// Defines operations for creating, suspending, resuming, and disposing cached objects.
/// </summary>
/// <typeparam name="TKey">The type of keys used to identify cached objects. Must be non-null.</typeparam>
/// <typeparam name="TValue">The type of values stored in the cache. Must be non-null.</typeparam>
public abstract class ObjectCacheProvider<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    /// <summary>
    /// Creates a new object or disposable reference for the specified key.
    /// </summary>
    /// <param name="key">The key identifying the object to create.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>Either a value object or a disposable reference to the object.</returns>
    public abstract Task<OneOf<TValue, IDisposableReference<TValue>>> CreateAsync(TKey key, CancellationToken ct);

    /// <summary>
    /// Suspends an object when it has no active references. Override to implement custom suspension logic.
    /// </summary>
    /// <param name="key">The key identifying the object to suspend.</param>
    /// <param name="value">The object to suspend.</param>
    /// <returns>A task that represents the asynchronous suspend operation.</returns>
    public virtual Task SuspendAsync(TKey key, TValue value) => Task.CompletedTask;

    /// <summary>
    /// Resumes an object when a new reference is requested. Override to implement custom resumption logic.
    /// </summary>
    /// <param name="key">The key identifying the object to resume.</param>
    /// <param name="value">The object to resume.</param>
    /// <returns>A task that represents the asynchronous resume operation.</returns>
    public virtual Task ResumeAsync(TKey key, TValue value) => Task.CompletedTask;

    /// <summary>
    /// Disposes an object when the cache is being disposed. Override to implement custom disposal logic.
    /// </summary>
    /// <param name="key">The key identifying the object to dispose.</param>
    /// <param name="value">The object to dispose.</param>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public virtual Task DisposeAsync(TKey key, TValue value) => Task.CompletedTask;
}
