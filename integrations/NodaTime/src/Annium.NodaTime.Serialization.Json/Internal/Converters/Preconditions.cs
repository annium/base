using System;
using NodaTime.Utility;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Helper static methods for argument/state validation. (Just the subset used within this library.)
/// </summary>
internal static class Preconditions
{
    /// <summary>
    /// Checks that the specified argument is not null.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="argument">The argument to check.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <returns>The original argument if not null.</returns>
    internal static T CheckNotNull<T>(T argument, string paramName)
        where T : class
    {
        return argument ?? throw new ArgumentNullException(paramName);
    }

    /// <summary>
    /// Checks that the specified condition is true, throwing an ArgumentException if it is false.
    /// </summary>
    /// <param name="expression">The condition to check.</param>
    /// <param name="parameter">The name of the parameter that caused the condition to fail.</param>
    /// <param name="message">The error message to include in the exception.</param>
    internal static void CheckArgument(bool expression, string parameter, string message)
    {
        if (!expression)
            throw new ArgumentException(message, parameter);
    }

    /// <summary>
    /// Checks that the specified condition is true, throwing an InvalidNodaDataException if it is false.
    /// </summary>
    /// <param name="expression">The condition to check.</param>
    /// <param name="message">The error message to include in the exception.</param>
    internal static void CheckData(bool expression, string message)
    {
        if (!expression)
        {
            throw new InvalidNodaDataException(message);
        }
    }
}
