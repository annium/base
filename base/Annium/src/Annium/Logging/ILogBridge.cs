namespace Annium.Logging;

public interface ILogBridge : ILogSubject
{
    string Type { get; }
}
