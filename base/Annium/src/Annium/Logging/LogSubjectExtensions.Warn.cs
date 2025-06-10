using System;
using System.Runtime.CompilerServices;

namespace Annium.Logging;

/// <summary>
/// Provides extension methods for logging warning-level messages for <see cref="ILogSubject"/> instances.
/// </summary>
public static class LogSubjectWarnExtensions
{
    /// <summary>
    /// Logs a warning-level message.
    /// </summary>
    /// <param name="subject">The log subject.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="file">The source file path (automatically provided).</param>
    /// <param name="member">The member name (automatically provided).</param>
    /// <param name="line">The line number (automatically provided).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, Array.Empty<object>());
    }

    /// <summary>
    /// Logs a warning-level message with one parameter.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <param name="subject">The log subject.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="x1">The first parameter value.</param>
    /// <param name="file">The source file path (automatically provided).</param>
    /// <param name="member">The member name (automatically provided).</param>
    /// <param name="line">The line number (automatically provided).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1>(
        this ILogSubject subject,
        string message,
        T1 x1,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new object?[] { x1 });
    }

    /// <summary>
    /// Logs a warning-level message with two parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <param name="subject">The log subject.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="x1">The first parameter value.</param>
    /// <param name="x2">The second parameter value.</param>
    /// <param name="file">The source file path (automatically provided).</param>
    /// <param name="member">The member name (automatically provided).</param>
    /// <param name="line">The line number (automatically provided).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1, T2>(
        this ILogSubject subject,
        string message,
        T1 x1,
        T2 x2,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new object?[] { x1, x2 });
    }

    /// <summary>
    /// Logs a warning-level message with three parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <param name="subject">The log subject.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="x1">The first parameter value.</param>
    /// <param name="x2">The second parameter value.</param>
    /// <param name="x3">The third parameter value.</param>
    /// <param name="file">The source file path (automatically provided).</param>
    /// <param name="member">The member name (automatically provided).</param>
    /// <param name="line">The line number (automatically provided).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1, T2, T3>(
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
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new object?[] { x1, x2, x3 });
    }

    /// <summary>
    /// Logs a warning-level message with four parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <param name="subject">The log subject.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="x1">The first parameter value.</param>
    /// <param name="x2">The second parameter value.</param>
    /// <param name="x3">The third parameter value.</param>
    /// <param name="x4">The fourth parameter value.</param>
    /// <param name="file">The source file path (automatically provided).</param>
    /// <param name="member">The member name (automatically provided).</param>
    /// <param name="line">The line number (automatically provided).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1, T2, T3, T4>(
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
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new object?[] { x1, x2, x3, x4 });
    }

    /// <summary>
    /// Logs a warning-level message with five parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <param name="subject">The log subject.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="x1">The first parameter value.</param>
    /// <param name="x2">The second parameter value.</param>
    /// <param name="x3">The third parameter value.</param>
    /// <param name="x4">The fourth parameter value.</param>
    /// <param name="x5">The fifth parameter value.</param>
    /// <param name="file">The source file path (automatically provided).</param>
    /// <param name="member">The member name (automatically provided).</param>
    /// <param name="line">The line number (automatically provided).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1, T2, T3, T4, T5>(
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
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new object?[] { x1, x2, x3, x4, x5 });
    }

    /// <summary>
    /// Logs a warning-level message with six parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth parameter.</typeparam>
    /// <param name="subject">The log subject.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="x1">The first parameter value.</param>
    /// <param name="x2">The second parameter value.</param>
    /// <param name="x3">The third parameter value.</param>
    /// <param name="x4">The fourth parameter value.</param>
    /// <param name="x5">The fifth parameter value.</param>
    /// <param name="x6">The sixth parameter value.</param>
    /// <param name="file">The source file path (automatically provided).</param>
    /// <param name="member">The member name (automatically provided).</param>
    /// <param name="line">The line number (automatically provided).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1, T2, T3, T4, T5, T6>(
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
        subject.Logger.Log(
            subject,
            file,
            member,
            line,
            LogLevel.Warn,
            message,
            new object?[] { x1, x2, x3, x4, x5, x6 }
        );
    }

    /// <summary>
    /// Logs a warning-level message with seven parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh parameter.</typeparam>
    /// <param name="subject">The log subject.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="x1">The first parameter value.</param>
    /// <param name="x2">The second parameter value.</param>
    /// <param name="x3">The third parameter value.</param>
    /// <param name="x4">The fourth parameter value.</param>
    /// <param name="x5">The fifth parameter value.</param>
    /// <param name="x6">The sixth parameter value.</param>
    /// <param name="x7">The seventh parameter value.</param>
    /// <param name="file">The source file path (automatically provided).</param>
    /// <param name="member">The member name (automatically provided).</param>
    /// <param name="line">The line number (automatically provided).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1, T2, T3, T4, T5, T6, T7>(
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
        subject.Logger.Log(
            subject,
            file,
            member,
            line,
            LogLevel.Warn,
            message,
            new object?[] { x1, x2, x3, x4, x5, x6, x7 }
        );
    }

    /// <summary>
    /// Logs a warning-level message with eight parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth parameter.</typeparam>
    /// <param name="subject">The log subject.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="x1">The first parameter value.</param>
    /// <param name="x2">The second parameter value.</param>
    /// <param name="x3">The third parameter value.</param>
    /// <param name="x4">The fourth parameter value.</param>
    /// <param name="x5">The fifth parameter value.</param>
    /// <param name="x6">The sixth parameter value.</param>
    /// <param name="x7">The seventh parameter value.</param>
    /// <param name="x8">The eighth parameter value.</param>
    /// <param name="file">The source file path (automatically provided).</param>
    /// <param name="member">The member name (automatically provided).</param>
    /// <param name="line">The line number (automatically provided).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1, T2, T3, T4, T5, T6, T7, T8>(
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
        subject.Logger.Log(
            subject,
            file,
            member,
            line,
            LogLevel.Warn,
            message,
            new object?[] { x1, x2, x3, x4, x5, x6, x7, x8 }
        );
    }
}
