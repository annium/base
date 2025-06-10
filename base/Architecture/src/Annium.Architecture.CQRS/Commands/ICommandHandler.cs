using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.CQRS.Commands;

/// <summary>
/// Handler interface for commands that return a response value
/// </summary>
/// <typeparam name="TRequest">The command request type</typeparam>
/// <typeparam name="TResponse">The response value type</typeparam>
public interface ICommandHandler<TRequest, TResponse>
    : IFinalRequestHandler<TRequest, IStatusResult<OperationStatus, TResponse>>
    where TRequest : ICommand;

/// <summary>
/// Handler interface for commands that don't return a response value
/// </summary>
/// <typeparam name="TRequest">The command request type</typeparam>
public interface ICommandHandler<TRequest> : IFinalRequestHandler<TRequest, IStatusResult<OperationStatus>>
    where TRequest : ICommand;
