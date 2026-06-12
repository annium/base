namespace Annium.Architecture.CQRS.Queries;

/// <summary>
/// Marker interface for query requests in the CQRS pattern. Carries no members;
/// exists solely as a type constraint on <c>IQueryHandler&lt;TRequest, TResponse&gt;</c> /
/// <c>IQueryHandler&lt;TRequest&gt;</c> so the mediator can statically
/// distinguish queries from commands.
/// </summary>
public interface IQuery;
