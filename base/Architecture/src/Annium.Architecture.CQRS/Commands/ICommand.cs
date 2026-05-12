namespace Annium.Architecture.CQRS.Commands;

/// <summary>
/// Marker interface for command requests in the CQRS pattern. Carries no members;
/// exists solely as a type constraint on <c>ICommandHandler&lt;TRequest&gt;</c> /
/// <c>ICommandHandler&lt;TRequest, TResponse&gt;</c> so the mediator can statically
/// distinguish commands from queries.
/// </summary>
public interface ICommand;
