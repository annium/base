using Annium.Logging.Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder ConfigureLoggingBridge(
        this ILoggingBuilder builder
    )
    {
        builder.ClearProviders();
        builder.Services.AddScoped<ILoggerProvider, LoggingBridgeProvider>();

        return builder;
    }
}