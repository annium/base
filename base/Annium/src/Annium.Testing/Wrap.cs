using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Testing;

/// <summary>
/// Provides methods to wrap actions for testing purposes.
/// </summary>
public static class Wrap
{
    /// <summary>
    /// Wraps a synchronous action with its expression for testing.
    /// </summary>
    /// <param name="action">The action to wrap.</param>
    /// <param name="delegateEx">The expression that produced the action.</param>
    /// <returns>A wrapped action.</returns>
    public static WrappedAction It(Action action, [CallerArgumentExpression(nameof(action))] string delegateEx = "") =>
        new(action, delegateEx);

    /// <summary>
    /// Wraps an asynchronous ValueTask action with its expression for testing.
    /// </summary>
    /// <param name="action">The asynchronous ValueTask action to wrap.</param>
    /// <param name="delegateEx">The expression that produced the action.</param>
    /// <returns>A wrapped asynchronous ValueTask action.</returns>
    public static WrappedValueTaskAction It(
        Func<ValueTask> action,
        [CallerArgumentExpression(nameof(action))] string delegateEx = ""
    ) => new(action, delegateEx);
}

/// <summary>
/// Represents a wrapped synchronous action for testing.
/// </summary>
public readonly struct WrappedAction
{
    /// <summary>
    /// Gets the action to execute.
    /// </summary>
    public readonly Action Execute;

    /// <summary>
    /// Gets the expression that produced the action.
    /// </summary>
    public readonly string Expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedAction"/> struct.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="expression">The expression that produced the action.</param>
    public WrappedAction(Action execute, string expression)
    {
        Execute = execute;
        Expression = expression;
    }
}

/// <summary>
/// Represents a wrapped asynchronous ValueTask action for testing.
/// </summary>
public readonly struct WrappedValueTaskAction
{
    /// <summary>
    /// Gets the asynchronous ValueTask action to execute.
    /// </summary>
    public readonly Func<ValueTask> Execute;

    /// <summary>
    /// Gets the expression that produced the action.
    /// </summary>
    public readonly string Expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedValueTaskAction"/> struct.
    /// </summary>
    /// <param name="execute">The asynchronous ValueTask action to execute.</param>
    /// <param name="expression">The expression that produced the action.</param>
    public WrappedValueTaskAction(Func<ValueTask> execute, string expression)
    {
        Execute = execute;
        Expression = expression;
    }
}
