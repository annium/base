using System;

namespace Annium.Core.Mediator.Internal;

internal static class Constants
{
    public static readonly Type PipeHandlerType = typeof(IPipeRequestHandler<,,,>);
    public static readonly string PipeHandlerHandleAsyncName = nameof(IPipeRequestHandler<int, int, int, int>.HandleAsync);
    public static readonly Type FinalHandlerType = typeof(IFinalRequestHandler<,>);
    public static readonly string FinalHandlerHandleAsyncName = nameof(IFinalRequestHandler<int, int>.HandleAsync);
    public static readonly Type HandlerInputType = typeof(IRequestHandlerInput<,>);
    public static readonly Type HandlerOutputType = typeof(IRequestHandlerOutput<,>);
}