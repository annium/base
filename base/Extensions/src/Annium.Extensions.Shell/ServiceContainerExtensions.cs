using System;
using Annium.Extensions.Shell;
using Annium.Extensions.Shell.Internal;
using Annium.Logging;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
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
