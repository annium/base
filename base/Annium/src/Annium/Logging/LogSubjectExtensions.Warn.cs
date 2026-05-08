using System.Runtime.CompilerServices;

#pragma warning disable LOG0002 // Forwarders intentionally pass caller info through to LogSubjectLogExtensions.Log.

namespace Annium.Logging;

/// <summary>
/// Provides extension methods for logging warning-level messages for <see cref="ILogSubject"/> instances.
/// All overloads forward to <see cref="LogSubjectLogExtensions.Log"/> with <see cref="LogLevel.Warn"/>.
/// </summary>
public static class LogSubjectWarnExtensions
{
    /// <summary>Logs a warning-level message.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    ) => subject.Log(LogLevel.Warn, message, file, member, line);

    /// <summary>Logs a warning-level message with one parameter.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1>(
        this ILogSubject subject,
        string message,
        T1 x1,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    ) => subject.Log(LogLevel.Warn, message, x1, file, member, line);

    /// <summary>Logs a warning-level message with two parameters.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn<T1, T2>(
        this ILogSubject subject,
        string message,
        T1 x1,
        T2 x2,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    ) => subject.Log(LogLevel.Warn, message, x1, x2, file, member, line);

    /// <summary>Logs a warning-level message with three parameters.</summary>
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
    ) => subject.Log(LogLevel.Warn, message, x1, x2, x3, file, member, line);

    /// <summary>Logs a warning-level message with four parameters.</summary>
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
    ) => subject.Log(LogLevel.Warn, message, x1, x2, x3, x4, file, member, line);

    /// <summary>Logs a warning-level message with five parameters.</summary>
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
    ) => subject.Log(LogLevel.Warn, message, x1, x2, x3, x4, x5, file, member, line);

    /// <summary>Logs a warning-level message with six parameters.</summary>
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
    ) => subject.Log(LogLevel.Warn, message, x1, x2, x3, x4, x5, x6, file, member, line);

    /// <summary>Logs a warning-level message with seven parameters.</summary>
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
    ) => subject.Log(LogLevel.Warn, message, x1, x2, x3, x4, x5, x6, x7, file, member, line);

    /// <summary>Logs a warning-level message with eight parameters.</summary>
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
    ) => subject.Log(LogLevel.Warn, message, x1, x2, x3, x4, x5, x6, x7, x8, file, member, line);
}
