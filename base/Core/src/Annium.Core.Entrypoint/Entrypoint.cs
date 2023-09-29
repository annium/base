using System;
using System.Runtime.Loader;
using System.Threading;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Entrypoint;

public class Entrypoint
{
    public static readonly Entrypoint Default = new();

    private bool _isAlreadyBuilt;

    private readonly IServiceProviderBuilder _serviceProviderBuilder =
        new ServiceProviderFactory().CreateBuilder(new ServiceCollection());

    public Entrypoint UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new()
    {
        _serviceProviderBuilder.UseServicePack<TServicePack>();

        return this;
    }

    public Entry Setup()
    {
        if (_isAlreadyBuilt)
            throw new InvalidOperationException("Entrypoint is already built");
        _isAlreadyBuilt = true;

        var gate = new ManualResetEventSlim(false);

        return new Entry(
            new ServiceProviderFactory().CreateServiceProvider(_serviceProviderBuilder),
            GetCancellationToken(gate),
            gate
        );
    }

    private static CancellationToken GetCancellationToken(ManualResetEventSlim gate)
    {
        var cts = new CancellationTokenSource();

        AssemblyLoadContext.Default.Unloading += _ => HandleEnd(cts, gate);
        Console.CancelKeyPress += (_, _) => HandleEnd(cts, gate);

        return cts.Token;
    }

    private static void HandleEnd(CancellationTokenSource cts, ManualResetEventSlim gate)
    {
        cts.Cancel();
        gate.Wait(CancellationToken.None);
    }
}