using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Base class for asynchronous commands with three configuration types
/// </summary>
/// <typeparam name="T1">The first configuration type</typeparam>
/// <typeparam name="T2">The second configuration type</typeparam>
/// <typeparam name="T3">The third configuration type</typeparam>
public abstract class AsyncCommand<T1, T2, T3> : CommandBase
    where T1 : new()
    where T2 : new()
    where T3 : new()
{
    /// <summary>
    /// Handles the command execution with three configurations
    /// </summary>
    /// <param name="cfg1">The first command configuration</param>
    /// <param name="cfg2">The second command configuration</param>
    /// <param name="cfg3">The third command configuration</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public abstract Task HandleAsync(T1 cfg1, T2 cfg2, T3 cfg3, CancellationToken ct);

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
            Console.WriteLine(Root.HelpBuilder.BuildHelp(id, description, typeof(T1), typeof(T2), typeof(T3)));
            return;
        }

        var cfg1 = Root.ConfigurationBuilder.Build<T1>(args);
        var cfg2 = Root.ConfigurationBuilder.Build<T2>(args);
        var cfg3 = Root.ConfigurationBuilder.Build<T3>(args);
        await HandleAsync(cfg1, cfg2, cfg3, ct);
    }
}
