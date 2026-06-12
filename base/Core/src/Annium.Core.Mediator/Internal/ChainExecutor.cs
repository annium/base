using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            parameters.Add(element.Next.NotNull().DynamicInvoke(provider, chain, index + 1).NotNull());

        var handler = element.Handler;
        // Both IPipeRequestHandler and IFinalRequestHandler name the method "HandleAsync"; the pipe/final
        // distinction is in the parameter count (3 with next when hasNext, else 2), captured in `parameters` above.
        // Resolve the method once per chain element (its parameter types are stable) and memoize it.
        var handleMethod = element.Handle;
        if (handleMethod is null)
        {
            handleMethod = handler
                .GetMethod(Constants.HandleAsyncName, parameters.Select(p => p.GetType()).ToArray())
                .NotNull();
            element.Handle = handleMethod;
        }

        // DoNotWrapExceptions: let a handler's own exceptions (e.g. OperationCanceledException) propagate
        // directly instead of being wrapped in a TargetInvocationException by reflection.
        var result = handleMethod
            .Invoke(
                provider.Resolve(handler),
                BindingFlags.DoNotWrapExceptions,
                binder: null,
                parameters.ToArray(),
                culture: null
            )
            .NotNull();
        await (Task)result;

        return result
            .GetType()
            // VSTHRD103: nameof(Task<>.Result) is a reflection member-name reference, not a blocking .Result access
#pragma warning disable VSTHRD103
            .GetProperty(nameof(Task<>.Result))
            .NotNull()
#pragma warning restore VSTHRD103
            .GetGetMethod()
            .NotNull()
            .Invoke(result, Array.Empty<object>())
            .NotNull();
    }
}
