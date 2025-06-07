using System;
using System.Threading.Tasks;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for delegate assertions in tests.
/// </summary>
public static class DelegateExtensions
{
    /// <summary>
    /// Asserts that the wrapped action throws an exception of the specified type.
    /// </summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="action">The wrapped action to execute.</param>
    /// <returns>The thrown exception.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the action does not throw the expected exception.</exception>
    public static TException Throws<TException>(this WrappedAction action)
        where TException : Exception
    {
        try
        {
            action.Execute();
        }
        catch (Exception exception)
        {
            return exception.Is<TException>();
        }

        throw new AssertionFailedException($"{action.Expression} has not thrown {typeof(TException).FriendlyName()}");
    }

    /// <summary>
    /// Asserts that the wrapped asynchronous action throws an exception of the specified type.
    /// </summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="action">The wrapped asynchronous action to execute.</param>
    /// <returns>The thrown exception.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the action does not throw the expected exception.</exception>
    public static async Task<TException> ThrowsAsync<TException>(this WrappedTaskAction action)
        where TException : Exception
    {
        try
        {
            await action.Execute();
        }
        catch (Exception exception)
        {
            return exception.Is<TException>();
        }

        throw new AssertionFailedException($"{action.Expression} has not thrown {typeof(TException).FriendlyName()}");
    }

    /// <summary>
    /// Asserts that the exception is of the specified type.
    /// </summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="value">The exception to check.</param>
    /// <returns>The exception cast to the specified type.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the exception is not of the specified type.</exception>
    private static TException Is<TException>(this Exception? value)
        where TException : Exception =>
        value.As<TException>(
            $"Returned result is type `{value?.GetType()}`, not derived from expected type `{typeof(TException)}`"
        );
}
