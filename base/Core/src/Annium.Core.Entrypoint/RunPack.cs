using System;
using System.Threading;

namespace Annium.Core.Entrypoint;

/// <summary>
/// Internal class for packaging runtime components together
/// </summary>
internal class RunPack
{
    /// <summary>
    /// Gets the synchronization gate for coordination
    /// </summary>
    public ManualResetEventSlim Gate { get; }

    /// <summary>
    /// Gets the cancellation token for shutdown coordination
    /// </summary>
    public CancellationToken Ct { get; }

    /// <summary>
    /// Gets the service provider for dependency resolution
    /// </summary>
    public IServiceProvider Provider { get; }

    /// <summary>
    /// Initializes a new instance of the RunPack class
    /// </summary>
    /// <param name="gate">The synchronization gate</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="provider">The service provider</param>
    public RunPack(ManualResetEventSlim gate, CancellationToken ct, IServiceProvider provider)
    {
        Gate = gate;
        Ct = ct;
        Provider = provider;
    }

    /// <summary>
    /// Deconstructs the RunPack into its component parts
    /// </summary>
    /// <param name="gate">The synchronization gate</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="provider">The service provider</param>
    public void Deconstruct(out ManualResetEventSlim gate, out CancellationToken ct, out IServiceProvider provider)
    {
        gate = Gate;
        ct = Ct;
        provider = Provider;
    }
}
