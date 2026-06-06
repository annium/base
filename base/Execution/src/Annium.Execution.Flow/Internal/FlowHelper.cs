using System;
using System.Threading.Tasks;

namespace Annium.Execution.Flow.Internal;

/// <summary>
/// Shared delegate-dispatch helper for the flow executors.
/// </summary>
internal static class FlowHelper
{
    /// <summary>
    /// Invokes a flow handler, dispatching on its concrete delegate type. Supports a synchronous
    /// <see cref="Action"/> and an asynchronous <see cref="Func{TResult}"/> returning <see cref="ValueTask"/>;
    /// a null handler (e.g. an absent stage rollback) is a no-op.
    /// </summary>
    /// <param name="handler">The delegate to invoke, or null for a no-op.</param>
    /// <returns>A task representing the invocation.</returns>
    public static async ValueTask ExecuteAsync(Delegate? handler)
    {
        if (handler is Func<ValueTask> handleAsync)
            await handleAsync();
        else if (handler is Action handleSync)
            handleSync();
        else if (handler is not null)
            throw new NotSupportedException($"Unsupported handler type: {handler.GetType()}");
    }
}
