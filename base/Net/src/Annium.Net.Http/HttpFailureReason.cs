namespace Annium.Net.Http;

/// <summary>
/// Represents the reason for an HTTP request failure
/// </summary>
public enum HttpFailureReason
{
    /// <summary>
    /// Undefined failure reason
    /// </summary>
    Undefined,

    /// <summary>
    /// Request was aborted
    /// </summary>
    Abort,

    /// <summary>
    /// Failed to parse response
    /// </summary>
    Parse,

    /// <summary>
    /// Exception occurred during request
    /// </summary>
    Exception,
}
