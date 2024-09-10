using System;
using Annium.Extensions.Shell;
using Annium.Extensions.Shell.Internal;
using Annium.Logging;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceCollection AddShell(this IServiceCollection services)
    {
        services.AddSingleton<IShell, Shell>();

        if (OperatingSystem.IsWindows())
            services.AddSingleton<Func<string[], IShellInstance>>(sp =>
                cmd => new WindowsShellInstance(cmd, sp.GetRequiredService<ILogger>())
            );
        else
            services.AddSingleton<Func<string[], IShellInstance>>(sp =>
                cmd => new UnixShellInstance(cmd, sp.GetRequiredService<ILogger>())
            );

        return services;
    }
}
