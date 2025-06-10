using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;

namespace Annium.Core.Entrypoint;

/// <summary>
/// Represents an application entry point with service provider, cancellation token, and synchronization gate
/// </summary>
public readonly record struct Entry(IServiceProvider Provider, CancellationToken Ct, ManualResetEventSlim _gate)
    : ILogSubject,
        IAsyncDisposable
{
    /// <summary>
    /// Gets the logger instance resolved from the service provider
    /// </summary>
    public ILogger Logger { get; } = Provider.Resolve<ILogger>();

    /// <summary>
    /// Gets the service provider for dependency resolution
    /// </summary>
    public readonly IServiceProvider Provider = Provider;

    /// <summary>
    /// Gets the cancellation token for coordinated shutdown
    /// </summary>
    public readonly CancellationToken Ct = Ct;

    /// <summary>
    /// Gets the synchronization gate for shutdown coordination
    /// </summary>
    private readonly ManualResetEventSlim _gate = _gate;

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

        this.Trace("done");
    }
}
