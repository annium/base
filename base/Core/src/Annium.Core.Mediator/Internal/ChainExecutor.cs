using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Core.Mediator.Internal;

/// <summary>
/// Executes mediator request handler chains
/// </summary>
internal static class ChainExecutor
{
    /// <summary>
    /// Executes a mediator chain starting from the specified index
    /// </summary>
    /// <param name="provider">Service provider for resolving handler instances</param>
    /// <param name="chain">Execution chain to process</param>
    /// <param name="request">Request object to process</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="index">Index of the chain element to start execution from</param>
    /// <returns>Result of the chain execution</returns>
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

        return result
            .GetType()
#pragma warning disable VSTHRD103
            .GetProperty(nameof(Task<int>.Result))!
#pragma warning restore VSTHRD103
            .GetGetMethod()!
            .Invoke(result, Array.Empty<object>())!;
    }
}
