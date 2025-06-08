namespace Annium.Architecture.Base;

/// <summary>
/// Represents the status of an operation indicating success, failure, or specific error conditions
/// </summary>
public enum OperationStatus
{
    NetworkError,
    Aborted,
    Timeout,
    BadRequest,
    Conflict,
    Forbidden,
    NotFound,
    Ok,
    UncaughtError,
}
