using System.Collections.Concurrent;

namespace Annium.Logging.Shared.Internal;

internal sealed class LogBridgeFactory : ILogBridgeFactory
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, ILogBridge> _instances = new();

    public LogBridgeFactory(ILogger logger)
    {
        _logger = logger;
    }

    public ILogBridge Get(string type)
    {
        return _instances.GetOrAdd(type, Create);
    }

    private ILogBridge Create(string type)
    {
        return new LogBridge(type, _logger);
    }
}
