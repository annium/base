using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Entrypoint;

/// <summary>
/// Provides a fluent API for configuring and setting up an application entry point
/// </summary>
public class Entrypoint
{
    /// <summary>
    /// Gets the default entrypoint instance
    /// </summary>
    public static readonly Entrypoint Default = new();

    /// <summary>
    /// Indicates whether the entrypoint has already been built
    /// </summary>
    private bool _isAlreadyBuilt;

    /// <summary>
    /// Cancellation source for application-wide shutdown. Field (not local) so it can be threaded
    /// into <see cref="IServiceProviderBuilder.BuildAsync"/> after the OS-event handlers that
    /// cancel it are wired.
    /// </summary>
    private CancellationTokenSource? _cts;

    /// <summary>
    /// The service provider builder for configuring dependencies
    /// </summary>
    private readonly IServiceProviderBuilder _serviceProviderBuilder = new ServiceProviderFactory().CreateBuilder(
        new ServiceCollection()
    );

    /// <summary>
    /// Configures the entrypoint to use the specified service pack
    /// </summary>
    /// <typeparam name="TServicePack">The type of service pack to use</typeparam>
    /// <returns>The current entrypoint instance for method chaining</returns>
    public Entrypoint UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new()
    {
        _serviceProviderBuilder.UseServicePack<TServicePack>();

        return this;
    }

    /// <summary>
    /// Asynchronously sets up and builds the entry point with configured services.
    /// Wires Console.CancelKeyPress and AssemblyLoadContext.Default.Unloading to cancel
    /// <see cref="Entry.Ct"/>, then builds the service provider directly through
    /// <see cref="IServiceProviderBuilder.BuildAsync"/> — no <see cref="ServiceProviderFactory"/>
    /// indirection on the Annium-native path.
    /// </summary>
    /// <returns>A configured Entry instance ready for use</returns>
    public async Task<Entry> SetupAsync()
    {
        if (_isAlreadyBuilt)
            throw new InvalidOperationException("Entrypoint is already built");

        var gate = new ManualResetEventSlim(false);
        _cts = new CancellationTokenSource();

        Action<AssemblyLoadContext> onUnloading = _ => HandleEnd(_cts, gate);
        ConsoleCancelEventHandler onCancelKeyPress = (_, _) => HandleEnd(_cts, gate);

        AssemblyLoadContext.Default.Unloading += onUnloading;
        Console.CancelKeyPress += onCancelKeyPress;

        Action cleanup = () =>
        {
            AssemblyLoadContext.Default.Unloading -= onUnloading;
            Console.CancelKeyPress -= onCancelKeyPress;
            _cts.Dispose();
        };

        ServiceProvider provider;
        try
        {
            provider = await _serviceProviderBuilder.BuildAsync(_cts.Token).ConfigureAwait(false);
        }
        catch
        {
            // Unhook handlers + dispose CTS so a failed Setup leaves no dangling subscriptions.
            // _isAlreadyBuilt stays false, so the caller may retry SetupAsync on this same
            // Entrypoint — ServiceProviderBuilder.BuildAsync is itself rebuildable after failure
            // (AC #5 sub-bullet "_isAlreadyBuilt set only on the normal return path").
            cleanup();
            throw;
        }

        // Flip the "already built" flag only on success — mirrors ServiceProviderBuilder.BuildAsync.
        _isAlreadyBuilt = true;

        return new Entry(provider, _cts.Token, gate, cleanup);
    }

    /// <summary>
    /// Handles application shutdown by cancelling the token and waiting for completion
    /// </summary>
    /// <param name="cts">The cancellation token source to cancel</param>
    /// <param name="gate">The gate to wait for completion</param>
    private static void HandleEnd(CancellationTokenSource cts, ManualResetEventSlim gate)
    {
        cts.Cancel();
        gate.Wait(CancellationToken.None);
    }
}
