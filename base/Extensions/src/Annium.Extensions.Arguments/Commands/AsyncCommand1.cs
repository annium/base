using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Base class for asynchronous commands with single configuration type
/// </summary>
/// <typeparam name="T">The configuration type</typeparam>
public abstract class AsyncCommand<T> : CommandBase
    where T : new()
{
    /// <summary>
    /// Handles the command execution with configuration
    /// </summary>
    /// <param name="cfg">The command configuration</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public abstract Task HandleAsync(T cfg, CancellationToken ct);

    /// <summary>
    /// Processes the command arguments and executes the command
    /// </summary>
    /// <param name="id">The command identifier</param>
    /// <param name="description">The command description</param>
    /// <param name="args">The command line arguments</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
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
