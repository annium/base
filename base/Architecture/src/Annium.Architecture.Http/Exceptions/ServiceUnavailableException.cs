using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Exception thrown when a request is aborted before completion. Maps to HTTP 503.
/// Surfaces <see cref="Annium.Architecture.Base.OperationStatus.Aborted"/> from a pipe handler.
/// </summary>
public class ServiceUnavailableException : HttpException
{
    /// <summary>
    /// Initializes a new instance of the ServiceUnavailableException class
    /// </summary>
    /// <param name="result">The result containing aborted-operation information</param>
    public ServiceUnavailableException(IResultBase result)
        : base(result) { }
}
