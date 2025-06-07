using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for attempting to make a generic method from a <see cref="MethodInfo"/>.
/// </summary>
public static class TryMakeGenericMethodExtension
{
    /// <summary>
    /// Attempts to make a generic method from the specified <see cref="MethodInfo"/> using the provided type arguments.
    /// </summary>
    /// <param name="method">The method info to make generic.</param>
    /// <param name="result">The resulting generic method, or null if the operation fails.</param>
    /// <param name="typeArguments">The type arguments to use for making the generic method.</param>
    /// <returns><c>true</c> if the generic method was successfully created; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="method"/> is null.</exception>
    public static bool TryMakeGenericMethod(this MethodInfo method, out MethodInfo? result, params Type[] typeArguments)
    {
        if (method is null)
            throw new ArgumentNullException(nameof(method));

        try
        {
            result = method.MakeGenericMethod(typeArguments);

            return true;
        }
        catch
        {
            result = null;

            return false;
        }
    }
}
