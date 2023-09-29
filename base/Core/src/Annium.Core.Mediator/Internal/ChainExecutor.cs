using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Core.Mediator.Internal;

internal static class ChainExecutor
{
    public static async Task<object> ExecuteAsync(
        IServiceProvider provider,
        IReadOnlyList<ChainElement> chain,
        object request,
        CancellationToken cancellationToken,
        int index = 0
    )
    {
        var hasNext = index < chain.Count - 1;
        var element = chain[index];

        var parameters = new List<object> { request, cancellationToken };
        if (hasNext)
            parameters.Add(element.Next!.DynamicInvoke(provider, chain, index + 1)!);

        var handler = element.Handler;
        var handleMethodName = hasNext ? Constants.FinalHandlerHandleAsyncName : Constants.PipeHandlerHandleAsyncName;
        var handleMethod = handler.GetMethod(handleMethodName, parameters.Select(p => p.GetType()).ToArray())!;
        var result = handleMethod.Invoke(provider.Resolve(handler), parameters.ToArray())!;
        await (Task)result;

        return result.GetType().GetProperty(nameof(Task<int>.Result))!.GetGetMethod()!.Invoke(result, Array.Empty<object>())!;
    }
}