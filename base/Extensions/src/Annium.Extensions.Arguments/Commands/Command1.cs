using System;
using System.Threading;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

public abstract class Command<T> : CommandBase
    where T : new()
{
    public abstract void Handle(T cfg, CancellationToken ct);

    public override void Process(string id, string description, string[] args, CancellationToken ct)
    {
        if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description, typeof(T)));
            return;
        }

        var cfg = Root.ConfigurationBuilder.Build<T>(args);

        Handle(cfg, ct);
    }
}