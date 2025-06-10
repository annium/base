using Annium.Architecture.CQRS.Commands;
using Annium.Architecture.CQRS.Queries;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;

namespace Annium.Architecture.CQRS;

/// <summary>
/// Extension methods for configuring CQRS handlers in the mediator
/// </summary>
public static class MediatorConfigurationExtensions
{
    /// <summary>
    /// Adds all command and query handlers found in the type manager to the mediator configuration
    /// </summary>
    /// <param name="cfg">The mediator configuration to extend</param>
    /// <param name="typeManager">The type manager to search for handlers</param>
    /// <returns>The updated mediator configuration</returns>
    public static MediatorConfiguration AddCommandQueryHandlers(
        this MediatorConfiguration cfg,
        ITypeManager typeManager
    )
    {
        foreach (var handler in typeManager.GetImplementations(typeof(ICommandHandler<,>)))
            cfg.AddHandler(handler);

        foreach (var handler in typeManager.GetImplementations(typeof(ICommandHandler<>)))
            cfg.AddHandler(handler);

        foreach (var handler in typeManager.GetImplementations(typeof(IQueryHandler<,>)))
            cfg.AddHandler(handler);

        return cfg;
    }
}
