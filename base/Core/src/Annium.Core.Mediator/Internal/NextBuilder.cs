using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Ex = System.Linq.Expressions.Expression;

namespace Annium.Core.Mediator.Internal;

/// <summary>
/// Builds delegate functions for invoking the next handler in execution chains
/// </summary>
internal class NextBuilder
{
    /// <summary>
    /// Cached reference to ChainExecutor.ExecuteAsync method
    /// </summary>
    private readonly MethodInfo _executeAsync = typeof(ChainExecutor).GetMethod(
        nameof(ChainExecutor.ExecuteAsync),
        BindingFlags.Public | BindingFlags.Static
    )!;

    /// <summary>
    /// Cached reference to Task.GetAwaiter method
    /// </summary>
    private readonly MethodInfo _getAwaiter = typeof(Task<object>).GetMethod(nameof(Task<>.GetAwaiter))!;

    /// <summary>
    /// Cached reference to TaskAwaiter.GetResult method
    /// </summary>
    private readonly MethodInfo _getResult = typeof(TaskAwaiter<object>).GetMethod(nameof(TaskAwaiter<>.GetResult))!;

    /// <summary>
    /// Cached reference to Task.FromResult method
    /// </summary>
    private readonly MethodInfo _fromResult = typeof(Task).GetMethod(nameof(Task.FromResult))!;

    /// <summary>
    /// Builds a delegate function for invoking the next handler in the chain
    /// </summary>
    /// <param name="input">Input type for the next handler</param>
    /// <param name="output">Output type for the next handler</param>
    /// <returns>Compiled delegate function for chain continuation</returns>
    public Delegate BuildNext(Type input, Type output)
    {
        var provider = Ex.Parameter(typeof(IServiceProvider));
        var chain = Ex.Parameter(typeof(IReadOnlyList<ChainElement>));
        var index = Ex.Parameter(typeof(int));
        var token = Ex.Parameter(typeof(CancellationToken));
        var request = Ex.Parameter(input);

        // get next lambda, as awaiting call to next
        var next = Ex.Lambda(
            Ex.Call(
                null,
                _fromResult.MakeGenericMethod(output),
                Ex.Convert(
                    Ex.Call(
                        Ex.Call(
                            Ex.Call(
                                null,
                                _executeAsync,
                                provider,
                                chain,
                                Ex.Convert(request, typeof(object)),
                                token,
                                index
                            ),
                            _getAwaiter
                        ),
                        _getResult
                    ),
                    output
                )
            ),
            request,
            token
        );

        return Ex.Lambda(next, provider, chain, index).Compile();
    }
}
