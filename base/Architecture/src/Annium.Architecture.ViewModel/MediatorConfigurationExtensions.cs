using Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;

// ReSharper disable CheckNamespace
namespace Annium.Core.Mediator;

public static class MediatorConfigurationExtensions
{
    public static MediatorConfiguration AddViewMappingHandlers(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(MappingEnumerablePipeHandler<,,>));
        cfg.AddHandler(typeof(MappingSinglePipeHandler<,,>));
        cfg.AddHandler(typeof(Architecture.ViewModel.Internal.PipeHandlers.Response.MappingEnumerablePipeHandler<,,>));
        cfg.AddHandler(typeof(Architecture.ViewModel.Internal.PipeHandlers.Response.MappingSinglePipeHandler<,,>));

        return cfg;
    }
}