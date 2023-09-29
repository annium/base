using System;
using NodaTime.Utility;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Helper static methods for argument/state validation. (Just the subset used within this library.)
/// </summary>
internal static class Preconditions
{
    internal static T CheckNotNull<T>(
        T argument,
        string paramName
    )
        where T : class
    {
        return argument ??
            throw new ArgumentNullException(paramName);
    }

    internal static void CheckArgument(
        bool expression,
        string parameter,
        string message
    )
    {
        if (!expression)
            throw new ArgumentException(message, parameter);
    }

    internal static void CheckData(
        bool expression,
        string message
    )
    {
        if (!expression)
        {
            throw new InvalidNodaDataException(message);
        }
    }
}