using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Pooling;

/// <summary>
/// Interface for managing cached objects with key-based retrieval
/// </summary>
/// <typeparam name="TKey">The type of cache keys</typeparam>
/// <typeparam name="TValue">The type of cached values</typeparam>
public interface IObjectCache<TKey, TValue> : IAsyncDisposable
    where TKey : notnull
    where TValue : notnull
{
    /// <summary>
    /// Gets a cached object asynchronously
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A disposable reference to the cached object</returns>
    Task<IDisposableReference<TValue>> GetAsync(TKey key, CancellationToken ct = default);
}
