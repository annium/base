using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Annium.Testing;

public static class DelegateExtensions
{
    public static TException Throws<TException>(this WrappedDelegate value)
        where TException : Exception
    {
        try
        {
            var result = value.Delegate.DynamicInvoke();
            if (result is Task { Exception: not null } task)
                return task.Exception.InnerExceptions.Single().Is<TException>();
        }
        catch (TargetInvocationException exception)
        {
            return exception.InnerException.NotNull().Is<TException>();
        }

        throw new AssertionFailedException($"{value.DelegateEx} has not thrown {typeof(TException).FriendlyName()}");
    }

    private static TException Is<TException>(this Exception? value)
        where TException : Exception =>
        value.As<TException>(
            $"Returned result is type `{value?.GetType()}`, not derived from expected type `{typeof(TException)}`"
        );
}
