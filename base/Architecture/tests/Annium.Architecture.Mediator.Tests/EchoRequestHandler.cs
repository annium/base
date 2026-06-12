using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Test handler that echoes the request back as a response.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
internal class EchoRequestHandler<TRequest> : IFinalRequestHandler<TRequest, IStatusResult<OperationStatus, TRequest>>
    where TRequest : IThrowing
{
    /// <summary>
    /// Handles the request by echoing it back or throwing an exception if requested.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A status result containing the original request.</returns>
    public Task<IStatusResult<OperationStatus, TRequest>> HandleAsync(TRequest request, CancellationToken ct)
    {
        if (request.Throw)
            throw new InvalidOperationException("TEST EXCEPTION");

        return Task.FromResult(Result.Status(OperationStatus.Ok, request));
    }
}

/// <summary>
/// Interface for objects that can indicate whether they should cause an exception.
/// </summary>
internal interface IThrowing
{
    /// <summary>
    /// Gets a value indicating whether an exception should be thrown.
    /// </summary>
    bool Throw { get; }
}

/// <summary>
/// Shared throwing request fixture: a request that carries a settable <see cref="IThrowing.Throw"/>
/// flag so tests can drive both the exception and the success paths of an exception pipe handler
/// without defining a per-test-class request type.
/// </summary>
internal class ThrowingRequest : IThrowing
{
    /// <summary>
    /// Gets or sets a value indicating whether an exception should be thrown.
    /// </summary>
    public bool Throw { get; set; }
}
