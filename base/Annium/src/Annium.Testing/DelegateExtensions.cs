using System;
using System.Threading.Tasks;

namespace Annium.Testing;

public static class DelegateExtensions
{
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

    private static TException Is<TException>(this Exception? value)
        where TException : Exception =>
        value.As<TException>(
            $"Returned result is type `{value?.GetType()}`, not derived from expected type `{typeof(TException)}`"
        );
}
