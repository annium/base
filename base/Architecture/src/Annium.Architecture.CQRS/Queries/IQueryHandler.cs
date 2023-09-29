using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.CQRS.Queries;

public interface IQueryHandler<TRequest, TResponse> :
    IFinalRequestHandler<TRequest, IStatusResult<OperationStatus, TResponse>>
    where TRequest : IQuery
{
}