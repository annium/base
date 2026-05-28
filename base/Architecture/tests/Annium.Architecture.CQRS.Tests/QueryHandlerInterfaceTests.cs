using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.CQRS.Commands;
using Annium.Architecture.CQRS.Queries;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.CQRS.Tests;

/// <summary>
/// Verifies the CQRS marker interfaces and handler shapes. In particular, that the no-data
/// <see cref="IQueryHandler{TRequest}"/> overload exists and is symmetric with
/// <see cref="ICommandHandler{TRequest}"/>.
/// </summary>
public class QueryHandlerInterfaceTests
{
    /// <summary>The no-data query handler can be defined and exercised end-to-end.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task NoDataQueryHandler_CanBeInvoked()
    {
        // arrange
        var handler = new ProbeQueryHandler();

        // act
        var result = await handler.HandleAsync(new ProbeQuery(), CancellationToken.None);

        // assert: the no-data IQueryHandler<TRequest> shape returns IStatusResult<OperationStatus>
        result.Status.Is(OperationStatus.Ok);
    }

    /// <summary>The two-arg query handler still works (regression guard).</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DataQueryHandler_CanBeInvoked()
    {
        // arrange
        var handler = new EchoQueryHandler();

        // act
        var result = await handler.HandleAsync(new EchoQuery { Value = "x" }, CancellationToken.None);

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.Data.Is("x");
    }

    /// <summary>Symmetric guard: the no-data command handler also still works.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task NoDataCommandHandler_CanBeInvoked()
    {
        // arrange
        var handler = new ProbeCommandHandler();

        // act
        var result = await handler.HandleAsync(new ProbeCommand(), CancellationToken.None);

        // assert
        result.Status.Is(OperationStatus.Ok);
    }

    /// <summary>The two-arg data-returning command handler returns both Ok status and the payload.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DataCommandHandler_CanBeInvoked()
    {
        // arrange
        const string payload = "hello";
        var handler = new DataCommandHandler();

        // act
        var result = await handler.HandleAsync(new DataCommand { Payload = payload }, CancellationToken.None);

        // assert: ICommandHandler<TRequest, TResponse> shape returns IStatusResult<OperationStatus, TResponse>
        result.Status.Is(OperationStatus.Ok);
        result.Data.Is(payload);
    }

    /// <summary>A minimal no-data query used as a probe in handler interface tests.</summary>
    private sealed class ProbeQuery : IQuery;

    /// <summary>Handles <see cref="ProbeQuery"/> and always returns an Ok status result.</summary>
    private sealed class ProbeQueryHandler : IQueryHandler<ProbeQuery>
    {
        /// <summary>Handles the probe query and returns an Ok status result.</summary>
        /// <param name="request">The probe query request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task whose result is an Ok <see cref="IStatusResult{OperationStatus}"/>.</returns>
        public Task<IStatusResult<OperationStatus>> HandleAsync(ProbeQuery request, CancellationToken ct) =>
            Task.FromResult<IStatusResult<OperationStatus>>(Result.Status(OperationStatus.Ok));
    }

    /// <summary>A query that carries a single string value and echoes it back through its handler.</summary>
    private sealed class EchoQuery : IQuery
    {
        /// <summary>Gets or sets the value to be echoed by the handler.</summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>Handles <see cref="EchoQuery"/> and returns the query's value as the result data.</summary>
    private sealed class EchoQueryHandler : IQueryHandler<EchoQuery, string>
    {
        /// <summary>Handles the echo query and returns the request value as the result data.</summary>
        /// <param name="request">The echo query request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task whose result is an Ok <see cref="IStatusResult{OperationStatus, String}"/> containing the echoed value.</returns>
        public Task<IStatusResult<OperationStatus, string>> HandleAsync(EchoQuery request, CancellationToken ct) =>
            Task.FromResult<IStatusResult<OperationStatus, string>>(Result.Status(OperationStatus.Ok, request.Value));
    }

    /// <summary>A minimal no-data command used as a probe in handler interface tests.</summary>
    private sealed class ProbeCommand : ICommand;

    /// <summary>Handles <see cref="ProbeCommand"/> and always returns an Ok status result.</summary>
    private sealed class ProbeCommandHandler : ICommandHandler<ProbeCommand>
    {
        /// <summary>Handles the probe command and returns an Ok status result.</summary>
        /// <param name="request">The probe command request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task whose result is an Ok <see cref="IStatusResult{OperationStatus}"/>.</returns>
        public Task<IStatusResult<OperationStatus>> HandleAsync(ProbeCommand request, CancellationToken ct) =>
            Task.FromResult<IStatusResult<OperationStatus>>(Result.Status(OperationStatus.Ok));
    }

    /// <summary>A command that carries a string payload and expects it echoed back as the response data.</summary>
    private sealed class DataCommand : ICommand
    {
        /// <summary>Gets or sets the payload to be returned by the handler.</summary>
        public string Payload { get; set; } = string.Empty;
    }

    /// <summary>Handles <see cref="DataCommand"/> and returns the command's payload as the result data.</summary>
    private sealed class DataCommandHandler : ICommandHandler<DataCommand, string>
    {
        /// <summary>Handles the data command and returns the request payload as the result data.</summary>
        /// <param name="request">The data command request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task whose result is an Ok <see cref="IStatusResult{OperationStatus, String}"/> containing the payload.</returns>
        public Task<IStatusResult<OperationStatus, string>> HandleAsync(DataCommand request, CancellationToken ct) =>
            Task.FromResult<IStatusResult<OperationStatus, string>>(Result.Status(OperationStatus.Ok, request.Payload));
    }
}
