using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments.Commands;

/// <summary>
/// Base class for synchronous commands with single configuration type
/// </summary>
/// <typeparam name="T">The configuration type</typeparam>
public abstract class Command<T> : CommandBase
    where T : new()
{
    /// <summary>
    /// Handles the command execution with configuration
    /// </summary>
    /// <param name="cfg">The command configuration</param>
    /// <param name="ct">The cancellation token</param>
    public abstract void Handle(T cfg, CancellationToken ct);

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
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description, typeof(T)));
            return Task.CompletedTask;
        }

        var cfg = Root.ConfigurationBuilder.Build<T>(args);

        Handle(cfg, ct);
        return Task.CompletedTask;
    }
}
