using System;
using System.Runtime.CompilerServices;

namespace Annium.Logging;

public static class LogSubjectErrorExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(
        this ILogSubject subject,
        Exception exception,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Error(subject, file, member, line, exception, Array.Empty<object>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(
        this ILogSubject subject,
        string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Error, message, Array.Empty<object>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error<T1>(
        this ILogSubject subject,
        string message,
        T1 x1,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Error, message, new object?[] { x1 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error<T1, T2>(
        this ILogSubject subject,
        string message,
        T1 x1,
        T2 x2,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Error, message, new object?[] { x1, x2 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error<T1, T2, T3>(
        this ILogSubject subject,
        string message,
        T1 x1,
        T2 x2,
        T3 x3,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Error, message, new object?[] { x1, x2, x3 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error<T1, T2, T3, T4>(
        this ILogSubject subject,
        string message,
        T1 x1,
        T2 x2,
        T3 x3,
        T4 x4,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Error, message, new object?[] { x1, x2, x3, x4 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error<T1, T2, T3, T4, T5>(
        this ILogSubject subject,
        string message,
        T1 x1,
        T2 x2,
        T3 x3,
        T4 x4,
        T5 x5,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Error, message, new object?[] { x1, x2, x3, x4, x5 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error<T1, T2, T3, T4, T5, T6>(
        this ILogSubject subject,
        string message,
        T1 x1,
        T2 x2,
        T3 x3,
        T4 x4,
        T5 x5,
        T6 x6,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Error, message, new object?[] { x1, x2, x3, x4, x5, x6 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error<T1, T2, T3, T4, T5, T6, T7>(
        this ILogSubject subject,
        string message,
        T1 x1,
        T2 x2,
        T3 x3,
        T4 x4,
        T5 x5,
        T6 x6,
        T7 x7,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Error, message, new object?[] { x1, x2, x3, x4, x5, x6, x7 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error<T1, T2, T3, T4, T5, T6, T7, T8>(
        this ILogSubject subject,
        string message,
        T1 x1,
        T2 x2,
        T3 x3,
        T4 x4,
        T5 x5,
        T6 x6,
        T7 x7,
        T8 x8,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Error, message, new object?[] { x1, x2, x3, x4, x5, x6, x7, x8 });
    }
}