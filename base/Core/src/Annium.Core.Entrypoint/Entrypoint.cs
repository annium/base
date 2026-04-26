using System;
using System.Runtime.Loader;
using System.Threading;
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
    /// Sets up and builds the entry point with configured services
    /// </summary>
    /// <returns>A configured Entry instance ready for use</returns>
    public Entry Setup()
    {
        if (_isAlreadyBuilt)
            throw new InvalidOperationException("Entrypoint is already built");
        _isAlreadyBuilt = true;

        var gate = new ManualResetEventSlim(false);
        var cts = new CancellationTokenSource();

        Action<AssemblyLoadContext> onUnloading = _ => HandleEnd(cts, gate);
        ConsoleCancelEventHandler onCancelKeyPress = (_, _) => HandleEnd(cts, gate);

        AssemblyLoadContext.Default.Unloading += onUnloading;
        Console.CancelKeyPress += onCancelKeyPress;

        Action cleanup = () =>
        {
            AssemblyLoadContext.Default.Unloading -= onUnloading;
            Console.CancelKeyPress -= onCancelKeyPress;
            cts.Dispose();
        };

        return new Entry(
            new ServiceProviderFactory().CreateServiceProvider(_serviceProviderBuilder),
            cts.Token,
            gate,
            cleanup
        );
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
