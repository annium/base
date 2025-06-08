using System;
using Annium.Extensions.Pooling.Internal.Storages;

namespace Annium.Extensions.Pooling.Internal.Loaders;

/// <summary>
/// Factory class for creating appropriate loader instances based on the specified loading mode.
/// </summary>
internal static class LoaderFactory
{
    /// <summary>
    /// Creates a loader instance based on the specified loading mode.
    /// </summary>
    /// <typeparam name="T">The type of objects the loader will handle.</typeparam>
    /// <param name="mode">The loading mode that determines the loader strategy.</param>
    /// <param name="factory">Factory function to create new instances of type T.</param>
    /// <param name="storage">Storage container for managing pooled objects.</param>
    /// <returns>A loader instance implementing the specified loading strategy.</returns>
    /// <exception cref="NotImplementedException">Thrown when an unsupported load mode is specified.</exception>
    public static ILoader<T> Create<T>(PoolLoadMode mode, Func<T> factory, IStorage<T> storage) =>
        mode switch
        {
            PoolLoadMode.Eager => new EagerLoader<T>(factory, storage),
            PoolLoadMode.Lazy => new LazyLoader<T>(factory, storage),
            _ => throw new NotImplementedException($"Unsupported load mode {mode}"),
        };
}
