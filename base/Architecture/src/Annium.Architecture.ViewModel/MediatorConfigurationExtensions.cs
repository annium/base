using Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;

// ReSharper disable CheckNamespace
namespace Annium.Core.Mediator;

/// <summary>
/// Extension methods for configuring view model mapping handlers in the mediator
/// </summary>
public static class MediatorConfigurationExtensions
{
    /// <summary>
    /// Adds view model mapping pipe handlers to the mediator configuration
    /// </summary>
    /// <param name="cfg">The mediator configuration to extend</param>
    /// <returns>The updated mediator configuration</returns>
    public static MediatorConfiguration AddViewMappingHandlers(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(MappingEnumerablePipeHandler<,,>));
        cfg.AddHandler(typeof(MappingSinglePipeHandler<,,>));
        cfg.AddHandler(typeof(Architecture.ViewModel.Internal.PipeHandlers.Response.MappingEnumerablePipeHandler<,,>));
        cfg.AddHandler(typeof(Architecture.ViewModel.Internal.PipeHandlers.Response.MappingSinglePipeHandler<,,>));

        return cfg;
    }
}
