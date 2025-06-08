using System.Collections.Concurrent;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Factory for creating and caching log bridge instances
/// </summary>
internal sealed class LogBridgeFactory : ILogBridgeFactory
{
    /// <summary>
    /// The logger instance for the factory
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Cache of log bridge instances by type
    /// </summary>
    private readonly ConcurrentDictionary<string, ILogBridge> _instances = new();

    public LogBridgeFactory(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates a log bridge instance for the specified type
    /// </summary>
    /// <param name="type">The type identifier for the log bridge</param>
    /// <returns>A log bridge instance</returns>
    public ILogBridge Get(string type)
    {
        return _instances.GetOrAdd(type, Create);
    }

    /// <summary>
    /// Creates a new log bridge instance for the specified type
    /// </summary>
    /// <param name="type">The type identifier for the log bridge</param>
    /// <returns>A new log bridge instance</returns>
    private ILogBridge Create(string type)
    {
        return new LogBridge(type, _logger);
    }
}
