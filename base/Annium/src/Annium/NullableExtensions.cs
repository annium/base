using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium;

public static class NullableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static T NotNull<T>(
        this T? value,
        [CallerArgumentExpression("value")] string expression = ""
    )
        where T : class
    {
        if (value is not null)
            return value;

        throw Exception(expression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>(
        this T? value,
        [CallerArgumentExpression("value")] string expression = ""
    )
        where T : struct
    {
        if (value.HasValue)
            return value.Value;

        throw Exception(expression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> NotNull<T>(
        this ValueTask<T?> task,
        [CallerArgumentExpression("task")] string expression = ""
    )
        where T : class
    {
        var value = await task;
        if (value is not null)
            return value;

        throw Exception(expression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> NotNull<T>(
        this ValueTask<T?> task,
        [CallerArgumentExpression("task")] string expression = ""
    )
        where T : struct
    {
        var value = await task;
        if (value.HasValue)
            return value.Value;

        throw Exception(expression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> NotNull<T>(
        this Task<T?> task,
        [CallerArgumentExpression("task")] string expression = ""
    )
        where T : class
    {
        var value = await task;
        if (value is not null)
            return value;

        throw Exception(expression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> NotNull<T>(
        this Task<T?> task,
        [CallerArgumentExpression("task")] string expression = ""
    )
        where T : struct
    {
        var value = await task;
        if (value.HasValue)
            return value.Value;

        throw Exception(expression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NullReferenceException Exception(string expression) =>
        new($"{expression} is null");
}