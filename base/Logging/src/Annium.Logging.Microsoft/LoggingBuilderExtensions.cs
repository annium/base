using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Annium.Logging.Microsoft;

/// <summary>
/// Extensions for ILoggingBuilder to configure logging bridge
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Configures the logging bridge for the logging builder
    /// </summary>
    /// <param name="builder">The logging builder to configure</param>
    /// <returns>The configured logging builder</returns>
    public static ILoggingBuilder ConfigureLoggingBridge(this ILoggingBuilder builder)
    {
        builder.ClearProviders();
        builder.Services.AddScoped<ILoggerProvider, LoggingBridgeProvider>();

        return builder;
    }
}
