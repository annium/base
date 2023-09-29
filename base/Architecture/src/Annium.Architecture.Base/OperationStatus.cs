namespace Annium.Architecture.Base;

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