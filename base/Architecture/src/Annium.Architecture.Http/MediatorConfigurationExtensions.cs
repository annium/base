using Annium.Architecture.Http.Internal.PipeHandlers.Request;
using Annium.Architecture.Http.Internal.PipeHandlers.RequestResponse;

// ReSharper disable CheckNamespace
namespace Annium.Core.Mediator;

/// <summary>
/// Extension methods for configuring HTTP status pipe handlers in the mediator
/// </summary>
public static class MediatorConfigurationExtensions
{
    /// <summary>
    /// Adds HTTP status pipe handlers to the mediator configuration
    /// </summary>
    /// <param name="cfg">The mediator configuration to extend</param>
    /// <returns>The updated mediator configuration</returns>
    public static MediatorConfiguration AddHttpStatusPipeHandler(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(HttpStatusPipeHandler<>));
        cfg.AddHandler(typeof(HttpStatusPipeHandler<,>));

        return cfg;
    }
}
