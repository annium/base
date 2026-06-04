using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;

namespace Annium.Core.Entrypoint;

/// <summary>
/// Represents an application entry point with service provider, cancellation token, and synchronization gate
/// </summary>
public readonly record struct Entry : ILogSubject, IAsyncDisposable
{
    /// <summary>
    /// Gets the application service provider.
    /// </summary>
    public IServiceProvider Provider { get; }

    /// <summary>
    /// Gets the application-wide cancellation token, cancelled on shutdown.
    /// </summary>
    public CancellationToken Ct { get; }

    /// <summary>
    /// Gets the logger instance resolved from the service provider.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Synchronization gate the shutdown handler blocks on until this entry is disposed.
    /// Owned by <see cref="Entrypoint"/>; coordination internal, not part of the public surface.
    /// </summary>
    private readonly ManualResetEventSlim _gate;

    /// <summary>
    /// Handler-unsubscription + CTS-disposal callback; invoked exclusively by <see cref="DisposeAsync"/>.
    /// </summary>
    private readonly Action _cleanup;

    /// <summary>
    /// Initializes a new <see cref="Entry"/>.
    /// </summary>
    /// <param name="provider">The application service provider.</param>
    /// <param name="ct">The application-wide cancellation token.</param>
    /// <param name="gate">The shutdown synchronization gate (owned by the entrypoint).</param>
    /// <param name="cleanup">The handler-unsubscription + CTS-disposal callback.</param>
    public Entry(IServiceProvider provider, CancellationToken ct, ManualResetEventSlim gate, Action cleanup)
    {
        Provider = provider;
        Ct = ct;
        _gate = gate;
        _cleanup = cleanup;
        Logger = provider.Resolve<ILogger>();
    }

    /// <summary>
    /// Deconstructs the entry into its core components
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="ct">The cancellation token</param>
    public void Deconstruct(out IServiceProvider provider, out CancellationToken ct)
    {
        provider = Provider;
        ct = Ct;
    }

    /// <summary>
    /// Asynchronously disposes the entry and its resources
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        if (Provider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else if (Provider is IDisposable disposable)
            disposable.Dispose();

        this.Trace("set gate");

        _gate.Set();

        this.Trace("unsubscribe handlers");

        _cleanup();

        this.Trace("done");
    }
}
