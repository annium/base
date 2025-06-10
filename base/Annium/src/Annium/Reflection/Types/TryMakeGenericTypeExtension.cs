using System;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for attempting to make a generic type from a <see cref="Type"/>.
/// </summary>
public static class TryMakeGenericTypeExtension
{
    /// <summary>
    /// Attempts to make a generic type from the specified type using the provided type arguments.
    /// </summary>
    /// <param name="type">The type to make generic.</param>
    /// <param name="result">When this method returns, contains the constructed generic type if successful; otherwise, null.</param>
    /// <param name="typeArguments">The type arguments to use for constructing the generic type.</param>
    /// <returns>True if the generic type was successfully constructed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool TryMakeGenericType(this Type type, out Type? result, params Type[] typeArguments)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        try
        {
            result = type.MakeGenericType(typeArguments);

            return true;
        }
        catch
        {
            result = null;

            return false;
        }
    }
}
