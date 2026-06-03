using System;

namespace Annium.Core.Mediator.Internal;

/// <summary>
/// Constants used throughout the mediator implementation
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Type definition for pipe request handlers
    /// </summary>
    public static readonly Type PipeHandlerType = typeof(IPipeRequestHandler<,,,>);

    /// <summary>
    /// Type definition for final request handlers
    /// </summary>
    public static readonly Type FinalHandlerType = typeof(IFinalRequestHandler<,>);

    /// <summary>
    /// Method name of the handler method on both pipe and final request handlers (both name it "HandleAsync")
    /// </summary>
    public static readonly string HandleAsyncName = nameof(IPipeRequestHandler<,,,>.HandleAsync);

    /// <summary>
    /// Type definition for request handler input interface
    /// </summary>
    public static readonly Type HandlerInputType = typeof(IRequestHandlerInput<,>);

    /// <summary>
    /// Type definition for request handler output interface
    /// </summary>
    public static readonly Type HandlerOutputType = typeof(IRequestHandlerOutput<,>);
}
