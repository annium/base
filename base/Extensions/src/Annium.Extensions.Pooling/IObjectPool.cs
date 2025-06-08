using System;

namespace Annium.Extensions.Pooling;

/// <summary>
/// Interface for managing a pool of reusable objects
/// </summary>
/// <typeparam name="T">The type of objects in the pool</typeparam>
public interface IObjectPool<T> : IDisposable
{
    /// <summary>
    /// Gets an object from the pool
    /// </summary>
    /// <returns>An object from the pool</returns>
    T Get();

    /// <summary>
    /// Returns an object to the pool
    /// </summary>
    /// <param name="item">The object to return</param>
    void Return(T item);
}
