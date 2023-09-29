using Annium.Architecture.Mediator.Internal.PipeHandlers;
using Annium.Architecture.Mediator.Internal.PipeHandlers.Request;
using Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse;

// ReSharper disable CheckNamespace
namespace Annium.Core.Mediator;

public static class MediatorConfigurationExtensions
{
    public static MediatorConfiguration AddCompositionHandler(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(CompositionPipeHandler<>));
        cfg.AddHandler(typeof(CompositionPipeHandler<,>));

        return cfg;
    }

    public static MediatorConfiguration AddExceptionHandler(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(ExceptionPipeHandler<>));
        cfg.AddHandler(typeof(ExceptionPipeHandler<,>));

        return cfg;
    }

    public static MediatorConfiguration AddLoggingHandler(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(LoggingPipeHandler<,>));

        return cfg;
    }

    public static MediatorConfiguration AddValidationHandler(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(ValidationPipeHandler<>));
        cfg.AddHandler(typeof(ValidationPipeHandler<,>));

        return cfg;
    }
}