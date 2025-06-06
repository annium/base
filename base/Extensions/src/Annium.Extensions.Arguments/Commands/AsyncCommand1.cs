using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

public abstract class AsyncCommand<T> : CommandBase
    where T : new()
{
    public abstract Task HandleAsync(T cfg, CancellationToken ct);

    public override async Task ProcessAsync(string id, string description, string[] args, CancellationToken ct)
    {
        if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description, typeof(T)));
            return;
        }

        var cfg = Root.ConfigurationBuilder.Build<T>(args);
        await HandleAsync(cfg, ct);
    }
}
