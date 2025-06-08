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
    /// <param name="cancellationToken">The cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The processed response</returns>
    public async Task<TResponseOut> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken,
        Func<TRequest, CancellationToken, Task<TResponseIn>> next
    )
    {
        var response = await next(request, cancellationToken);

        return GetResponse(response);
    }

    /// <summary>
    /// Converts the input response to the output response type
    /// </summary>
    /// <param name="response">The input response</param>
    /// <returns>The converted output response</returns>
    protected abstract TResponseOut GetResponse(TResponseIn response);

    /// <summary>
    /// Handles operation status by throwing appropriate HTTP exceptions for error statuses
    /// </summary>
    /// <param name="status">The operation status to handle</param>
    /// <param name="result">The result containing error information</param>
    protected void HandleStatus(OperationStatus status, IResultBase result)
    {
        if (status == OperationStatus.BadRequest)
            throw new ValidationException(result);

        if (status == OperationStatus.Forbidden)
            throw new ForbiddenException(result);

        if (status == OperationStatus.NotFound)
            throw new NotFoundException(result);

        if (status == OperationStatus.Conflict)
            throw new ConflictException(result);

        if (status == OperationStatus.UncaughtError)
            throw new ServerException(result);

        // if mapping fails - it's critical error
        if (status != OperationStatus.Ok)
            throw new ServerException(result);
    }
}
