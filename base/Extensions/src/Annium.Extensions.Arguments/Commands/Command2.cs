using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

public abstract class Command<T1, T2> : CommandBase
    where T1 : new()
    where T2 : new()
{
    public abstract void Handle(T1 cfg1, T2 cfg2, CancellationToken ct);

    public override Task ProcessAsync(string id, string description, string[] args, CancellationToken ct)
    {
        if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description, typeof(T1), typeof(T2)));
            return Task.CompletedTask;
        }

        var cfg1 = Root.ConfigurationBuilder.Build<T1>(args);
        var cfg2 = Root.ConfigurationBuilder.Build<T2>(args);

        Handle(cfg1, cfg2, ct);
        return Task.CompletedTask;
    }
}
