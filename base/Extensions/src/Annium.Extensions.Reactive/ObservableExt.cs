using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Reactive.Internal.Creation.Instance;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Provides extension methods for creating custom observable instances
/// </summary>
public static class ObservableExt
{
    #region Instance

    // public static IObservableInstance<T> Dynamic<T>(Func<ObserverContext<T>, Task<Func<Task>>> factory)
    // {
    //     return new DynamicObservableInstance<T>(factory);
    // }

    /// <summary>
    /// Creates a static observable instance that runs asynchronously
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="factory">Factory function that produces values and returns a disposal function</param>
    /// <param name="ct">Cancellation token for the observable operation</param>
    /// <param name="logger">Logger instance for tracking observable lifecycle</param>
    /// <returns>An observable that executes the factory function asynchronously</returns>
    public static IObservable<T> StaticAsyncInstance<T>(
        Func<ObserverContext<T>, Task<Func<Task>>> factory,
        CancellationToken ct,
        ILogger logger
    )
    {
        return new StaticObservableInstance<T>(factory, true, ct, logger);
    }

    /// <summary>
    /// Creates a static observable instance that runs synchronously
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="factory">Factory function that produces values and returns a disposal function</param>
    /// <param name="ct">Cancellation token for the observable operation</param>
    /// <param name="logger">Logger instance for tracking observable lifecycle</param>
    /// <returns>An observable that executes the factory function synchronously</returns>
    public static IObservable<T> StaticSyncInstance<T>(
        Func<ObserverContext<T>, Task<Func<Task>>> factory,
        CancellationToken ct,
        ILogger logger
    )
    {
        return new StaticObservableInstance<T>(factory, false, ct, logger);
    }

    #endregion
}
