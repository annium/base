namespace Annium.Architecture.Base;

/// <summary>
/// Represents the status of an operation indicating success, failure, or a specific error condition.
/// </summary>
/// <remarks>
/// The zero value is <see cref="None"/> rather than any failure or success status, so that a
/// default-initialised <see cref="OperationStatus"/> (e.g. an uninitialised struct field) does not
/// silently mean an outcome. Numeric values are assigned explicitly so the wire/serialised form is
/// stable independent of source ordering; downstream consumers that persisted previous (implicit)
/// values must remap accordingly when adopting this version.
/// </remarks>
public enum OperationStatus
{
    /// <summary>
    /// Default value indicating no status has been set. Should not be returned by a completed handler.
    /// </summary>
    None = 0,

    /// <summary>
    /// The operation failed because of a network-level error (connection refused, DNS, transport).
    /// </summary>
    NetworkError = 1,

    /// <summary>
    /// The operation was aborted before completion (e.g. cancelled by the caller or a watchdog).
    /// </summary>
    Aborted = 2,

    /// <summary>
    /// The operation exceeded its time budget.
    /// </summary>
    Timeout = 3,

    /// <summary>
    /// The request was malformed or failed validation; the caller should fix the input and retry.
    /// </summary>
    BadRequest = 4,

    /// <summary>
    /// The operation conflicts with the current state of the target resource.
    /// </summary>
    Conflict = 5,

    /// <summary>
    /// The caller is authenticated but lacks permission for the requested operation.
    /// </summary>
    Forbidden = 6,

    /// <summary>
    /// The requested resource could not be located.
    /// </summary>
    NotFound = 7,

    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Ok = 8,

    /// <summary>
    /// An unexpected exception was caught by the pipeline; the original exception is logged but not
    /// surfaced to the caller. Indicates a programming error or unhandled condition.
    /// </summary>
    UncaughtError = 9,

    /// <summary>
    /// The caller is not authenticated; credentials are missing or invalid.
    /// </summary>
    Unauthorized = 10,
}
