using Annium.Architecture.Base;
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

        return Result.New(response.Data).Join(response);
    }
}
