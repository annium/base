namespace Annium.Logging.Shared.Internal;

internal class LogSubject : ILogSubject
{
    public ILogger Logger { get; }

    public LogSubject(ILogger logger)
    {
        Logger = logger;
    }
}