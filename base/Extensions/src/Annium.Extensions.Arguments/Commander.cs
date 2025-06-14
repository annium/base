using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Extensions.Arguments;

/// <summary>
/// Static class for running command groups with dependency injection
/// </summary>
public static class Commander
{
    /// <summary>
    /// Runs a command group with the specified arguments
    /// </summary>
    /// <typeparam name="TGroup">The type of command group to run</typeparam>
    /// <param name="provider">The service provider for dependency injection</param>
    /// <param name="args">The command line arguments</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task RunAsync<TGroup>(IServiceProvider provider, string[] args, CancellationToken ct = default)
        where TGroup : Group, ICommandDescriptor
    {
        var group = provider.Resolve<TGroup>();
        group.SetRoot(provider.Resolve<Root>());
        await group.ProcessAsync(TGroup.Id, TGroup.Description, args, ct);
    }
}
