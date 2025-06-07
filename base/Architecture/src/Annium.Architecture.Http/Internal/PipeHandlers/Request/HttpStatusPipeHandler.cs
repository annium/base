using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Internal.PipeHandlers.Request;

/// <summary>
/// Pipe handler that converts operation status results to HTTP exceptions for request-only operations
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
internal class HttpStatusPipeHandler<TRequest>
    : HttpStatusPipeHandlerBase<TRequest, IStatusResult<OperationStatus>, IResult>,
        IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus>, IResult>
{
    /// <summary>
    /// Converts a status result to a basic result, throwing HTTP exceptions for error statuses
    /// </summary>
    /// <param name="response">The status result to convert</param>
    /// <returns>A basic result</returns>
    protected override IResult GetResponse(IStatusResult<OperationStatus> response)
    {
        HandleStatus(response.Status, response);

        return Result.New().Join(response);
    }
}
