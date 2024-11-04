using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

public abstract class AsyncCommand : CommandBase
{
    public abstract Task HandleAsync(CancellationToken ct);

    public override async Task ProcessAsync(string id, string description, string[] args, CancellationToken ct)
    {
        if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description));
            return;
        }

        await HandleAsync(ct);
    }
}
