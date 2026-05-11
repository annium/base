using Annium.Architecture.Base;
using Annium.Architecture.Http.Exceptions;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Internal.PipeHandlers.RequestResponse;

/// <summary>
/// Pipe handler that converts operation status results to HTTP exceptions for request-response operations
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal class HttpStatusPipeHandler<TRequest, TResponse>
    : HttpStatusPipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>, IResult<TResponse>>,
        IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IResult<TResponse>>
{
    /// <summary>
    /// Converts a status result with data to a basic result with data, throwing HTTP exceptions for error statuses
    /// </summary>
    /// <param name="response">The status result to convert</param>
    /// <returns>A basic result with data</returns>
    protected override IResult<TResponse> GetResponse(IStatusResult<OperationStatus, TResponse> response)
    {
        HandleStatus(response.Status, response);

        // Defense in depth: an upstream handler that returns Status=Ok with Data=null is a programming
        // error (TResponse has no `notnull` constraint, so the type system cannot enforce this). Surface
        // it as a ServerException (HTTP 500) instead of propagating a null-Data result downstream.
        if (response.Data is null)
            throw new ServerException(response);

        return Result.Create(response.Data).Join(response);
    }
}
