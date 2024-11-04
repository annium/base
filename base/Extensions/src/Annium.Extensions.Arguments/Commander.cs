using System;
using System.Threading;
using Annium.Core.DependencyInjection;

namespace Annium.Extensions.Arguments;

public static class Commander
{
    public static async System.Threading.Tasks.Task RunAsync<TGroup>(
        IServiceProvider provider,
        string[] args,
        CancellationToken ct = default
    )
        where TGroup : Group, ICommandDescriptor
    {
        var group = provider.Resolve<TGroup>();
        group.SetRoot(provider.Resolve<Root>());
        await group.ProcessAsync(TGroup.Id, TGroup.Description, args, ct);
    }
}
