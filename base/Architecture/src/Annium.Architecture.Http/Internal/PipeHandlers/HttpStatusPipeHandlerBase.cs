using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.Http.Exceptions;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Internal.PipeHandlers;

/// <summary>
/// Base class for pipe handlers that convert operation status results to HTTP exceptions
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponseIn">The input response type</typeparam>
/// <typeparam name="TResponseOut">The output response type</typeparam>
internal abstract class HttpStatusPipeHandlerBase<TRequest, TResponseIn, TResponseOut>
{
    /// <summary>
    /// Handles the request by executing the next handler and processing the response
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The processed response</returns>
    public async Task<TResponseOut> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponseIn>> next
    )
    {
        var response = await next(request, ct);

        return GetResponse(response);
    }

    /// <summary>
    /// Converts the input response to the output response type
    /// </summary>
    /// <param name="response">The input response</param>
    /// <returns>The converted output response</returns>
    protected abstract TResponseOut GetResponse(TResponseIn response);

    /// <summary>
    /// Handles operation status by throwing the matching HTTP exception. The mapping is
    /// total over <see cref="OperationStatus"/>: <c>Ok</c> is a no-op; every other defined
    /// member maps to a dedicated exception (4xx for client errors, 5xx for server/upstream
    /// errors). Future enum members fall through to <see cref="ServerException"/> (HTTP 500)
    /// rather than going unhandled.
    /// </summary>
    /// <param name="status">The operation status to handle</param>
    /// <param name="result">The result containing error information</param>
    protected void HandleStatus(OperationStatus status, IResultBase result)
    {
        Exception? toThrow = status switch
        {
            OperationStatus.Ok => null,
            OperationStatus.BadRequest => new ValidationException(result),
            OperationStatus.Unauthorized => new UnauthorizedException(result),
            OperationStatus.Forbidden => new ForbiddenException(result),
            OperationStatus.NotFound => new NotFoundException(result),
            OperationStatus.Conflict => new ConflictException(result),
            OperationStatus.NetworkError => new BadGatewayException(result),
            OperationStatus.Aborted => new ServiceUnavailableException(result),
            OperationStatus.Timeout => new GatewayTimeoutException(result),
            _ => new ServerException(result),
        };

        if (toThrow is not null)
            throw toThrow;
    }
}
