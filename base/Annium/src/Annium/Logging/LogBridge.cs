namespace Annium.Logging;

public class LogBridge : ILogBridge
{
    public ILogger Logger { get; }
    public string Type { get; }

    public LogBridge(string type, ILogger logger)
    {
        Logger = logger;
        Type = type;
    }
}
