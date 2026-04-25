using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Exception thrown when an upstream gateway dependency is unreachable. Maps to HTTP 502.
/// Surfaces <see cref="Annium.Architecture.Base.OperationStatus.NetworkError"/> from a pipe handler.
/// </summary>
public class BadGatewayException : HttpException
{
    /// <summary>
    /// Initializes a new instance of the BadGatewayException class
    /// </summary>
    /// <param name="result">The result containing network-error information</param>
    public BadGatewayException(IResultBase result)
        : base(result) { }
}
