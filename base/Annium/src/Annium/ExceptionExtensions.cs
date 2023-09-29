using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Annium;

public static class ExceptionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Rethrow<T>(this T exception)
        where T : Exception
    {
        ExceptionDispatchInfo.Capture(exception).Throw();

        return exception;
    }
}