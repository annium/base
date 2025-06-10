using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments.Commands;

/// <summary>
/// Base class for synchronous commands with two configuration types
/// </summary>
/// <typeparam name="T1">The first configuration type</typeparam>
/// <typeparam name="T2">The second configuration type</typeparam>
public abstract class Command<T1, T2> : CommandBase
    where T1 : new()
    where T2 : new()
{
    /// <summary>
    /// Handles the command execution with two configurations
    /// </summary>
    /// <param name="cfg1">The first command configuration</param>
    /// <param name="cfg2">The second command configuration</param>
    /// <param name="ct">The cancellation token</param>
    public abstract void Handle(T1 cfg1, T2 cfg2, CancellationToken ct);

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
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description, typeof(T1), typeof(T2)));
            return Task.CompletedTask;
        }

        var cfg1 = Root.ConfigurationBuilder.Build<T1>(args);
        var cfg2 = Root.ConfigurationBuilder.Build<T2>(args);

        Handle(cfg1, cfg2, ct);
        return Task.CompletedTask;
    }
}
