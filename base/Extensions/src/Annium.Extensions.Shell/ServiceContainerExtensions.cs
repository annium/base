using System;
using Annium.Extensions.Shell;
using Annium.Extensions.Shell.Internal;
using Annium.Logging;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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
    public static IServiceContainer AddShell(this IServiceContainer services)
    {
        services.Add<IShell, Shell>().Singleton();

        if (OperatingSystem.IsWindows())
            services
                .Add<Func<string[], IShellInstance>>(sp =>
                    cmd => new WindowsShellInstance(cmd, sp.GetRequiredService<ILogger>())
                )
                .AsSelf()
                .Singleton();
        else
            services
                .Add<Func<string[], IShellInstance>>(sp =>
                    cmd => new UnixShellInstance(cmd, sp.GetRequiredService<ILogger>())
                )
                .AsSelf()
                .Singleton();

        return services;
    }
}
