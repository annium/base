using Annium.Architecture.CQRS.Commands;
using Annium.Architecture.CQRS.Queries;
using Annium.Core.Runtime.Types;

// ReSharper disable CheckNamespace
namespace Annium.Core.Mediator;

public static class MediatorConfigurationExtensions
{
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