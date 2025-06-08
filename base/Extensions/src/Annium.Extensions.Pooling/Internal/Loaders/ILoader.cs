namespace Annium.Extensions.Pooling.Internal.Loaders;

/// <summary>
/// Defines a contract for loading objects from a pool storage using different strategies.
/// </summary>
/// <typeparam name="T">The type of objects to load from the pool.</typeparam>
internal interface ILoader<T>
{
    /// <summary>
    /// Gets an object from the pool, creating it if necessary based on the loader's strategy.
    /// </summary>
    /// <returns>An object of type T from the pool.</returns>
    T Get();
}
