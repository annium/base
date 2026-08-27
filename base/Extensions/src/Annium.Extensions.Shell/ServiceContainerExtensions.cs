using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Shell.Internal;
using Annium.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Shell;

/// <summary>
/// Extension methods for registering shell command execution services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers cross-platform shell command execution services with the service container
    /// </summary>
    /// <param name="services">The service container to register services with</param>
    /// <returns>The service container for method chaining</returns>
    /// <remarks>
    /// Both registrations are singletons, and the factory closure resolves <c>ILogger</c> - which the
    /// logging package registers as scoped - from the provider it was built with. Shell command logs
    /// therefore carry the root scope's logging context, not that of whatever scope ran the command.
    ///
    /// This is a deliberate standstill rather than an oversight: making the shell scoped would push the
    /// same captured-scope problem into consumers, which register singletons depending on <c>IShell</c>,
    /// where the container cannot see it either. Resolving it properly means giving the shell a logger at
    /// the point of use rather than at construction.
    /// </remarks>
    public static IServiceContainer AddShell(this IServiceContainer services)
    {
        services.Add<IShell, Internal.Shell>().Singleton();

        // one implementation for every platform: the Windows path used to route the command line through
        // cmd.exe, and once it stopped doing that nothing about starting the process differed
        services
            .Add<Func<string[], IShellInstance>>(sp => cmd => new ShellInstance(cmd, sp.GetRequiredService<ILogger>()))
            .AsSelf()
            .Singleton();

        return services;
    }
}
