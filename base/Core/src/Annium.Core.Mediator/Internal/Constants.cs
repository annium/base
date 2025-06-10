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
    /// Method name for pipe handler HandleAsync method
    /// </summary>
    public static readonly string PipeHandlerHandleAsyncName = nameof(
        IPipeRequestHandler<int, int, int, int>.HandleAsync
    );

    /// <summary>
    /// Type definition for final request handlers
    /// </summary>
    public static readonly Type FinalHandlerType = typeof(IFinalRequestHandler<,>);

    /// <summary>
    /// Method name for final handler HandleAsync method
    /// </summary>
    public static readonly string FinalHandlerHandleAsyncName = nameof(IFinalRequestHandler<int, int>.HandleAsync);

    /// <summary>
    /// Type definition for request handler input interface
    /// </summary>
    public static readonly Type HandlerInputType = typeof(IRequestHandlerInput<,>);

    /// <summary>
    /// Type definition for request handler output interface
    /// </summary>
    public static readonly Type HandlerOutputType = typeof(IRequestHandlerOutput<,>);
}
