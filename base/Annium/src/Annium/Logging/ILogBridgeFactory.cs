namespace Annium.Logging;

/// <summary>
/// Factory interface for creating log bridges with specified type information.
/// </summary>
public interface ILogBridgeFactory
{
    /// <summary>
    /// Creates a new log bridge instance for the specified type.
    /// </summary>
    /// <param name="type">The type identifier for the log bridge.</param>
    /// <returns>A new instance of <see cref="ILogBridge"/> configured for the specified type.</returns>
    ILogBridge Get(string type);
}
