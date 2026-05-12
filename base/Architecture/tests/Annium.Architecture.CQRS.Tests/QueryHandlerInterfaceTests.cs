using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.CQRS.Commands;
using Annium.Architecture.CQRS.Queries;
using Annium.Core.Mediator;
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

    private sealed class ProbeQuery : IQuery;

    private sealed class ProbeQueryHandler : IQueryHandler<ProbeQuery>
    {
        public Task<IStatusResult<OperationStatus>> HandleAsync(ProbeQuery request, CancellationToken ct) =>
            Task.FromResult<IStatusResult<OperationStatus>>(Result.Status(OperationStatus.Ok));
    }

    private sealed class EchoQuery : IQuery
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed class EchoQueryHandler : IQueryHandler<EchoQuery, string>
    {
        public Task<IStatusResult<OperationStatus, string>> HandleAsync(EchoQuery request, CancellationToken ct) =>
            Task.FromResult<IStatusResult<OperationStatus, string>>(Result.Status(OperationStatus.Ok, request.Value));
    }

    private sealed class ProbeCommand : ICommand;

    private sealed class ProbeCommandHandler : ICommandHandler<ProbeCommand>
    {
        public Task<IStatusResult<OperationStatus>> HandleAsync(ProbeCommand request, CancellationToken ct) =>
            Task.FromResult<IStatusResult<OperationStatus>>(Result.Status(OperationStatus.Ok));
    }
}
