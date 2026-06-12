using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// A final request handler used exclusively by request-only pipe handler tests.
/// Returns <c>IStatusResult&lt;OperationStatus&gt;</c> (no data) so that the mediator resolves
/// the request-only pipe handler variants rather than the two-arg request-response variants.
/// Throws <see cref="System.InvalidOperationException"/> when <c>IThrowing.Throw</c> is <c>true</c>.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
internal class RequestVariantEchoRequestHandler<TRequest>
    : IFinalRequestHandler<TRequest, IStatusResult<OperationStatus>>
    where TRequest : IThrowing
{
    /// <summary>
    /// Handles the request by returning an <c>Ok</c> status result, or throwing when requested.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An <c>Ok</c> status result with no data payload.</returns>
    public Task<IStatusResult<OperationStatus>> HandleAsync(TRequest request, CancellationToken ct)
    {
        if (request.Throw)
            throw new System.InvalidOperationException("TEST EXCEPTION");

        return Task.FromResult(Result.Status(OperationStatus.Ok));
    }
}
