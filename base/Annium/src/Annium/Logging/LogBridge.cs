namespace Annium.Logging;

/// <summary>
/// Represents a concrete implementation of <see cref="ILogBridge"/> that bridges a logger and a log subject with a specific type.
/// </summary>
public class LogBridge : ILogBridge
{
    /// <summary>
    /// Gets the logger instance associated with this bridge.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the type identifier for this log bridge.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogBridge"/> class.
    /// </summary>
    /// <param name="type">The type identifier for the log bridge.</param>
    /// <param name="logger">The logger instance to associate with this bridge.</param>
    public LogBridge(string type, ILogger logger)
    {
        Logger = logger;
        Type = type;
    }
}
