using System;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

public static class DelegateExtensions
{
    public static TException Throws<TException>(this WrappedDelegate value)
        where TException : Exception
    {
        try
        {
            var result = value.Delegate.DynamicInvoke();
            if (result is Task task) task.Await();
        }
        catch (TargetInvocationException exception)
        {
            return exception.InnerException!.Is<TException>();
        }

        throw new AssertionFailedException($"{value.DelegateEx} has not thrown {typeof(TException).FriendlyName()}");
    }

    private static TException Is<TException>(this Exception? value) where TException : Exception =>
        value.As<TException>($"Returned result is type `{value?.GetType()}`, not derived from expected type `{typeof(TException)}`");
}