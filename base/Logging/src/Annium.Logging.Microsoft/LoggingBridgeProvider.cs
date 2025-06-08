using Annium.Logging.Shared;
using Microsoft.Extensions.Logging;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;

namespace Annium.Logging.Microsoft;

/// <summary>
/// Logger provider that creates logging bridges for Microsoft.Extensions.Logging integration
/// </summary>
internal class LoggingBridgeProvider : ILoggerProvider
{
    /// <summary>
    /// The log sentry bridge for forwarding log messages
    /// </summary>
    private readonly ILogSentryBridge _sentryBridge;

    public LoggingBridgeProvider(ILogSentryBridge sentryBridge)
    {
        _sentryBridge = sentryBridge;
    }

    /// <summary>
    /// Creates a new ILogger instance
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger</param>
    /// <returns>A new ILogger instance</returns>
    public IMicrosoftLogger CreateLogger(string categoryName)
    {
        return new LoggingBridge(_sentryBridge, categoryName);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
    /// </summary>
    public void Dispose() { }
}
