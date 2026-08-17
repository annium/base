namespace Annium.Logging;

/// <summary>
/// Severity levels a log message can be written with, ordered from most to least verbose.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Fine-grained diagnostic detail, useful only when tracing execution step by step.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Diagnostic detail useful while developing or debugging.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Informational messages describing normal application flow.
    /// </summary>
    Info = 2,

    /// <summary>
    /// Unexpected situations that do not abort the current operation.
    /// </summary>
    Warn = 3,

    /// <summary>
    /// Failures that abort the current operation.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Logging turned off — no message passes the level filter.
    /// </summary>
    None = 5,
}
