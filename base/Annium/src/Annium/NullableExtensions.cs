using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium;

public static class NullableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static T NotNull<T>(this T? value, [CallerArgumentExpression(nameof(value))] string expression = "")
        where T : class
    {
        if (value is not null)
            return value;

        throw Exception(expression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>(this T? value, [CallerArgumentExpression(nameof(value))] string expression = "")
        where T : struct
    {
        if (value.HasValue)
            return value.Value;

        throw Exception(expression);
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NullReferenceException Exception(string expression) => new($"{expression} is null");
}
