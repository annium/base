using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Annium;

/// <summary>
/// Provides extension methods for working with exceptions.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Rethrows an exception while preserving its original stack trace.
    /// </summary>
    /// <typeparam name="T">The type of the exception.</typeparam>
    /// <param name="exception">The exception to rethrow.</param>
    /// <returns>The same exception instance.</returns>
    /// <remarks>
    /// This method uses <see cref="ExceptionDispatchInfo"/> to preserve the original stack trace
    /// when rethrowing the exception, which is not possible with a simple throw statement.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Rethrow<T>(this T exception)
        where T : Exception
    {
        ExceptionDispatchInfo.Capture(exception).Throw();

        return exception;
    }
}
