using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Base class for all command implementations
/// </summary>
public abstract class CommandBase
{
    /// <summary>
    /// Gets the root configuration instance
    /// </summary>
    internal Root Root
    {
        get => field ?? throw new InvalidOperationException("Root is not set");
        private set;
    }

    /// <summary>
    /// Processes the command with the specified arguments
    /// </summary>
    /// <param name="id">The command identifier</param>
    /// <param name="description">The command description</param>
    /// <param name="args">The command line arguments</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public abstract Task ProcessAsync(string id, string description, string[] args, CancellationToken ct);

    /// <summary>
    /// Sets the root configuration for the command
    /// </summary>
    /// <param name="root">The root configuration instance</param>
    internal void SetRoot(Root root)
    {
        Root = root;
    }
}
