using System;
using System.Threading;

namespace Annium.Core.Entrypoint;

internal class RunPack
{
    public ManualResetEventSlim Gate { get; }

    public CancellationToken Ct { get; }

    public IServiceProvider Provider { get; }

    public RunPack(
        ManualResetEventSlim gate,
        CancellationToken ct,
        IServiceProvider provider
    )
    {
        Gate = gate;
        Ct = ct;
        Provider = provider;
    }

    public void Deconstruct(
        out ManualResetEventSlim gate,
        out CancellationToken ct,
        out IServiceProvider provider
    )
    {
        gate = Gate;
        ct = Ct;
        provider = Provider;
    }
}