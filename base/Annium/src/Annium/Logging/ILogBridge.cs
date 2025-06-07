namespace Annium.Logging;

/// <summary>
/// Represents a bridge between a logger and a log subject, providing type information for logging.
/// </summary>
public interface ILogBridge : ILogSubject
{
    /// <summary>
    /// Gets the type identifier for this log bridge.
    /// </summary>
    string Type { get; }
}
