using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.CQRS.Commands;

public interface ICommandHandler<TRequest, TResponse> :
    IFinalRequestHandler<TRequest, IStatusResult<OperationStatus, TResponse>>
    where TRequest : ICommand
{
}

public interface ICommandHandler<TRequest> :
    IFinalRequestHandler<TRequest, IStatusResult<OperationStatus>>
    where TRequest : ICommand
{
}