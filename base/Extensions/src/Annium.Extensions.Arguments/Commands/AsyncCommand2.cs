using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;
using Annium.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

public abstract class AsyncCommand<T1, T2> : CommandBase
    where T1 : new()
    where T2 : new()
{
    public abstract Task HandleAsync(T1 cfg1, T2 cfg2, CancellationToken ct);

    public override void Process(string id, string description, string[] args, CancellationToken ct)
    {
        if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description, typeof(T1), typeof(T2)));
            return;
        }

        var cfg1 = Root.ConfigurationBuilder.Build<T1>(args);
        var cfg2 = Root.ConfigurationBuilder.Build<T2>(args);

        HandleAsync(cfg1, cfg2, ct).Await();
    }
}