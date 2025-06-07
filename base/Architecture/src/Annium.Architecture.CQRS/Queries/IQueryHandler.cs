using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.CQRS.Queries;

/// <summary>
/// Handler interface for queries that return a response value
/// </summary>
/// <typeparam name="TRequest">The query request type</typeparam>
/// <typeparam name="TResponse">The response value type</typeparam>
public interface IQueryHandler<TRequest, TResponse>
    : IFinalRequestHandler<TRequest, IStatusResult<OperationStatus, TResponse>>
    where TRequest : IQuery;
