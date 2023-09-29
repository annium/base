using System;
using System.Threading;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

public abstract class Command<T1, T2, T3> : CommandBase
    where T1 : new()
    where T2 : new()
    where T3 : new()
{
    public abstract void Handle(T1 cfg1, T2 cfg2, T3 cfg3, CancellationToken ct);

    public override void Process(string id, string description, string[] args, CancellationToken ct)
    {
        if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description, typeof(T1), typeof(T2), typeof(T3)));
            return;
        }

        var cfg1 = Root.ConfigurationBuilder.Build<T1>(args);
        var cfg2 = Root.ConfigurationBuilder.Build<T2>(args);
        var cfg3 = Root.ConfigurationBuilder.Build<T3>(args);

        Handle(cfg1, cfg2, cfg3, ct);
    }
}