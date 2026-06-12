using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Tests.Threading.Tasks;

/// <summary>
/// Contains unit tests for the <see cref="ValueTaskSet"/> class. Covers the AggregateException path
/// that was previously 0% covered (review T2).
/// </summary>
public class ValueTaskSetTests
{
    /// <summary>
    /// Verifies that WhenAll over a list of successful ValueTask&lt;T&gt; returns all results in order.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_T_AllSucceed_ReturnsResults()
    {
        var tasks = new List<ValueTask<int>> { Vt(1), Vt(2), Vt(3) };

        var result = await ValueTaskSet.WhenAll(tasks);

        result.Length.Is(3);
        result[0].Is(1);
        result[1].Is(2);
        result[2].Is(3);
    }

    /// <summary>
    /// Verifies that WhenAll over a list with one faulting ValueTask&lt;T&gt; throws an AggregateException
    /// whose inner exception is the original.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_T_OneFaults_ThrowsAggregateException()
    {
        var tasks = new List<ValueTask<int>> { Vt(1), VtFault<int>(new InvalidOperationException("boom")), Vt(3) };

        var ex = await Wrap.It(async () => await ValueTaskSet.WhenAll(tasks)).ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Count.Is(1);
        ex.InnerExceptions[0].As<InvalidOperationException>().Message.Is("boom");
    }

    /// <summary>
    /// Verifies that WhenAll over a list with multiple faulting ValueTask&lt;T&gt; aggregates all inner exceptions.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_T_MultipleFault_AggregateContainsAll()
    {
        var tasks = new List<ValueTask<int>>
        {
            VtFault<int>(new InvalidOperationException("a")),
            Vt(2),
            VtFault<int>(new ArgumentException("b")),
        };

        var ex = await Wrap.It(async () => await ValueTaskSet.WhenAll(tasks)).ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Count.Is(2);
        ex.InnerExceptions[0].As<InvalidOperationException>();
        ex.InnerExceptions[1].As<ArgumentException>();
    }

    /// <summary>
    /// Verifies that WhenAll over the no-result variant throws an AggregateException when a task faults.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_NoResult_OneFaults_Throws()
    {
        var tasks = new List<ValueTask> { Vt(), VtFault(new InvalidOperationException("nope")), Vt() };

        var ex = await Wrap.It(async () => await ValueTaskSet.WhenAll(tasks)).ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Count.Is(1);
        ex.InnerExceptions[0].Message.Is("nope");
    }

    /// <summary>
    /// Verifies that WhenAll over an empty list returns an empty result without throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_T_EmptyList_ReturnsEmpty()
    {
        var result = await ValueTaskSet.WhenAll(new List<ValueTask<int>>());

        result.Length.Is(0);
    }

    /// <summary>
    /// Creates a completed ValueTask&lt;T&gt; with the given value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A completed ValueTask containing the value.</returns>
    private static ValueTask<T> Vt<T>(T value) => new(value);

    /// <summary>
    /// Creates a completed (non-result) ValueTask.
    /// </summary>
    /// <returns>A completed ValueTask.</returns>
    private static ValueTask Vt() => ValueTask.CompletedTask;

    /// <summary>
    /// Creates a ValueTask&lt;T&gt; that immediately faults with the given exception.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="ex">The exception to fault with.</param>
    /// <returns>A faulted ValueTask.</returns>
    private static ValueTask<T> VtFault<T>(Exception ex) => new(Task.FromException<T>(ex));

    /// <summary>
    /// Creates a ValueTask that immediately faults with the given exception.
    /// </summary>
    /// <param name="ex">The exception to fault with.</param>
    /// <returns>A faulted ValueTask.</returns>
    private static ValueTask VtFault(Exception ex) => new(Task.FromException(ex));
}
