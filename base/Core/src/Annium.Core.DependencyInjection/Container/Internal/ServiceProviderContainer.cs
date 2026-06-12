using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Default <see cref="IServiceProviderContainer"/> implementation. Wraps a built
/// <see cref="ServiceProvider"/>, delegating every (keyed and non-keyed) resolution to it, and fires
/// the captured <c>OnDisposed</c> callback before disposing the wrapped provider.
/// </summary>
internal sealed class ServiceProviderContainer : IServiceProviderContainer
{
    /// <summary>
    /// The wrapped Microsoft service provider that performs the actual resolution.
    /// </summary>
    private readonly ServiceProvider _inner;

    /// <summary>
    /// The container's <c>OnDisposed</c> invocation captured at build time, run once before the
    /// wrapped provider is disposed. Nulled after it runs so a double dispose cannot re-invoke it.
    /// </summary>
    private Func<ValueTask>? _onDisposed;

    /// <summary>
    /// Dispose guard: <c>0</c> until the first Dispose/DisposeAsync, then <c>1</c>. Ensures the
    /// <c>OnDisposed</c> callbacks and the wrapped provider's disposal run at most once.
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new <see cref="ServiceProviderContainer"/>.
    /// </summary>
    /// <param name="inner">The wrapped Microsoft service provider.</param>
    /// <param name="onDisposed">Callback invoked once, before <paramref name="inner"/> is disposed.</param>
    public ServiceProviderContainer(ServiceProvider inner, Func<ValueTask> onDisposed)
    {
        _inner = inner;
        _onDisposed = onDisposed;
    }

    /// <summary>
    /// Resolves a service, delegating to the wrapped provider.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <returns>The resolved service, or <see langword="null"/> if not registered.</returns>
    public object? GetService(Type serviceType) => _inner.GetService(serviceType);

    /// <summary>
    /// Resolves a keyed service, delegating to the wrapped provider.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <param name="serviceKey">The key the service was registered with.</param>
    /// <returns>The resolved service, or <see langword="null"/> if not registered.</returns>
    public object? GetKeyedService(Type serviceType, object? serviceKey) =>
        ((IKeyedServiceProvider)_inner).GetKeyedService(serviceType, serviceKey);

    /// <summary>
    /// Resolves a required keyed service, delegating to the wrapped provider.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <param name="serviceKey">The key the service was registered with.</param>
    /// <returns>The resolved service.</returns>
    public object GetRequiredKeyedService(Type serviceType, object? serviceKey) =>
        ((IKeyedServiceProvider)_inner).GetRequiredKeyedService(serviceType, serviceKey);

    /// <summary>
    /// Asynchronously disposes the container: runs the captured <c>OnDisposed</c> callbacks, then
    /// disposes the wrapped provider. Idempotent.
    /// </summary>
    /// <returns>A task that completes once disposal finishes.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        var onDisposed = _onDisposed;
        _onDisposed = null;
        if (onDisposed is not null)
            await onDisposed().ConfigureAwait(false);

        await _inner.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously disposes the container: runs the captured <c>OnDisposed</c> callbacks (blocking
    /// at this disposal boundary, since the callbacks are async), then disposes the wrapped provider.
    /// Idempotent. Prefer <see cref="DisposeAsync"/> where an async context is available.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        var onDisposed = _onDisposed;
        _onDisposed = null;
        if (onDisposed is not null)
            // sync-over-async is unavoidable on the synchronous IDisposable path; it runs only at the
            // disposal boundary (shutdown / test teardown), never on a hot path.
#pragma warning disable VSTHRD002
            onDisposed().AsTask().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

        _inner.Dispose();
    }
}
