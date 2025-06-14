using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Base class for synchronous commands without configuration
/// </summary>
public abstract class Command : CommandBase
{
    /// <summary>
    /// Handles the command execution
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    public abstract void Handle(CancellationToken ct);

    /// <summary>
    /// Processes the command arguments and executes the command
    /// </summary>
    /// <param name="id">The command identifier</param>
    /// <param name="description">The command description</param>
    /// <param name="args">The command line arguments</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A completed task</returns>
    public override Task ProcessAsync(string id, string description, string[] args, CancellationToken ct)
    {
        if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description));
            return Task.CompletedTask;
        }

        Handle(ct);
        return Task.CompletedTask;
    }
}
