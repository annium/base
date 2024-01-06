namespace Annium.Logging;

public interface ILogBridgeFactory
{
    ILogBridge Get(string type);
}