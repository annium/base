using Annium.Architecture.Mediator.Internal.PipeHandlers;
using Annium.Architecture.Mediator.Internal.PipeHandlers.Request;
using Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse;
using Annium.Core.Mediator;

namespace Annium.Architecture.Mediator;

/// <summary>
/// Extension methods for configuring architecture pipe handlers in the mediator
/// </summary>
public static class MediatorConfigurationExtensions
{
    /// <summary>
    /// Adds composition pipe handlers to the mediator configuration
    /// </summary>
    /// <param name="cfg">The mediator configuration to extend</param>
    /// <returns>The updated mediator configuration</returns>
    public static MediatorConfiguration AddCompositionHandler(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(CompositionPipeHandler<>));
        cfg.AddHandler(typeof(CompositionPipeHandler<,>));

        return cfg;
    }

    /// <summary>
    /// Adds exception pipe handlers to the mediator configuration
    /// </summary>
    /// <param name="cfg">The mediator configuration to extend</param>
    /// <returns>The updated mediator configuration</returns>
    public static MediatorConfiguration AddExceptionHandler(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(ExceptionPipeHandler<>));
        cfg.AddHandler(typeof(ExceptionPipeHandler<,>));

        return cfg;
    }

    /// <summary>
    /// Adds logging pipe handlers to the mediator configuration
    /// </summary>
    /// <param name="cfg">The mediator configuration to extend</param>
    /// <returns>The updated mediator configuration</returns>
    public static MediatorConfiguration AddLoggingHandler(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(LoggingPipeHandler<,>));

        return cfg;
    }

    /// <summary>
    /// Adds validation pipe handlers to the mediator configuration
    /// </summary>
    /// <param name="cfg">The mediator configuration to extend</param>
    /// <returns>The updated mediator configuration</returns>
    public static MediatorConfiguration AddValidationHandler(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(ValidationPipeHandler<>));
        cfg.AddHandler(typeof(ValidationPipeHandler<,>));

        return cfg;
    }
}
