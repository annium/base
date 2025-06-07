namespace Annium.Logging;

/// <summary>
/// Represents an object that can be used as a subject for logging operations.
/// </summary>
public interface ILogSubject
{
    /// <summary>
    /// Gets the logger instance associated with this subject.
    /// </summary>
    ILogger Logger { get; }
}
