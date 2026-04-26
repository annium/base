using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Exception thrown when an upstream gateway dependency times out. Maps to HTTP 504.
/// Surfaces <see cref="Annium.Architecture.Base.OperationStatus.Timeout"/> from a pipe handler.
/// </summary>
public class GatewayTimeoutException : HttpException
{
    /// <summary>
    /// Initializes a new instance of the GatewayTimeoutException class
    /// </summary>
    /// <param name="result">The result containing timeout information</param>
    public GatewayTimeoutException(IResultBase result)
        : base(result) { }
}
