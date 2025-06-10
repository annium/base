using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium;

/// <summary>
/// Provides extension methods for working with nullable types.
/// </summary>
public static class NullableExtensions
{
    /// <summary>
    /// Ensures that a nullable reference type is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value to check.</param>
    /// <param name="expression">The expression that was evaluated to produce the value.</param>
    /// <returns>The non-null value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static T NotNull<T>(this T? value, [CallerArgumentExpression(nameof(value))] string expression = "")
        where T : class
    {
        if (value is not null)
            return value;

        throw Exception(expression);
    }

    /// <summary>
    /// Ensures that a nullable value type is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value to check.</param>
    /// <param name="expression">The expression that was evaluated to produce the value.</param>
    /// <returns>The non-null value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>(this T? value, [CallerArgumentExpression(nameof(value))] string expression = "")
        where T : struct
    {
        if (value.HasValue)
            return value.Value;

        throw Exception(expression);
    }

    /// <summary>
    /// Ensures that a nullable reference type from a ValueTask is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="task">The task that produces the nullable value.</param>
    /// <param name="expression">The expression that was evaluated to produce the task.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the non-null value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the task result is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> NotNullAsync<T>(
        this ValueTask<T?> task,
        [CallerArgumentExpression(nameof(task))] string expression = ""
    )
        where T : class
    {
        var value = await task;
        if (value is not null)
            return value;

        throw Exception(expression);
    }

    /// <summary>
    /// Ensures that a nullable value type from a ValueTask is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="task">The task that produces the nullable value.</param>
    /// <param name="expression">The expression that was evaluated to produce the task.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the non-null value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the task result is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> NotNullAsync<T>(
        this ValueTask<T?> task,
        [CallerArgumentExpression(nameof(task))] string expression = ""
    )
        where T : struct
    {
        var value = await task;
        if (value.HasValue)
            return value.Value;

        throw Exception(expression);
    }

    /// <summary>
    /// Ensures that a nullable reference type from a Task is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="task">The task that produces the nullable value.</param>
    /// <param name="expression">The expression that was evaluated to produce the task.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the non-null value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the task result is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> NotNullAsync<T>(
        this Task<T?> task,
        [CallerArgumentExpression(nameof(task))] string expression = ""
    )
        where T : class
    {
#pragma warning disable VSTHRD003
        var value = await task;
#pragma warning restore VSTHRD003
        if (value is not null)
            return value;

        throw Exception(expression);
    }

    /// <summary>
    /// Ensures that a nullable value type from a Task is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="task">The task that produces the nullable value.</param>
    /// <param name="expression">The expression that was evaluated to produce the task.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the non-null value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the task result is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> NotNullAsync<T>(
        this Task<T?> task,
        [CallerArgumentExpression(nameof(task))] string expression = ""
    )
        where T : struct
    {
#pragma warning disable VSTHRD003
        var value = await task;
#pragma warning restore VSTHRD003
        if (value.HasValue)
            return value.Value;

        throw Exception(expression);
    }

    /// <summary>
    /// Creates a NullReferenceException with a message that includes the expression that was null.
    /// </summary>
    /// <param name="expression">The expression that was evaluated to produce the null value.</param>
    /// <returns>A NullReferenceException with a descriptive message.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NullReferenceException Exception(string expression) => new($"{expression} is null");
}
