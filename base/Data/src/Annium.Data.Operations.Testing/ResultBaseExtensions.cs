using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Annium.Testing;

namespace Annium.Data.Operations.Testing;

/// <summary>
/// Testing extensions for result base types
/// </summary>
public static class ResultBaseExtensions
{
    /// <summary>
    /// Asserts that the result has no errors
    /// </summary>
    /// <param name="value">The result to check</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void HasNoErrors(
        this IResultBase value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (value.HasErrors)
            throw new AssertionFailedException(
                message
                    ?? $"{value.WrapWithExpression(valueEx)} contains errors: {Environment.NewLine}{value.ErrorState()}"
            );
    }

    /// <summary>
    /// Asserts that the result has errors
    /// </summary>
    /// <param name="value">The result to check</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void HasErrors(
        this IResultBase value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (value.IsOk)
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} contains no errors");
    }

    /// <summary>
    /// Asserts that the boolean result represents success.
    /// </summary>
    /// <param name="value">The boolean result to check</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void IsSuccess(
        this IBooleanResult value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (!value.IsSuccess)
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} is not Success");
    }

    /// <summary>
    /// Asserts that the boolean result with data represents success.
    /// </summary>
    /// <typeparam name="TD">The type of the result data</typeparam>
    /// <param name="value">The boolean result to check</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void IsSuccess<TD>(
        this IBooleanResult<TD> value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (!value.IsSuccess)
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} is not Success");
    }

    /// <summary>
    /// Asserts that the boolean result represents failure.
    /// </summary>
    /// <param name="value">The boolean result to check</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void IsFailure(
        this IBooleanResult value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (value.IsSuccess)
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} is not Failure");
    }

    /// <summary>
    /// Asserts that the boolean result with data represents failure.
    /// </summary>
    /// <typeparam name="TD">The type of the result data</typeparam>
    /// <param name="value">The boolean result to check</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void IsFailure<TD>(
        this IBooleanResult<TD> value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (value.IsSuccess)
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} is not Failure");
    }

    /// <summary>
    /// Asserts that the status result has the expected status.
    /// </summary>
    /// <typeparam name="TS">The type of the status</typeparam>
    /// <param name="value">The status result to check</param>
    /// <param name="expected">The expected status value</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void HasStatus<TS>(
        this IStatusResult<TS> value,
        TS expected,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (!EqualityComparer<TS>.Default.Equals(value.Status, expected))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)}.Status ({value.Status}) is not {expected}"
            );
    }

    /// <summary>
    /// Asserts that the status result with data has the expected status.
    /// </summary>
    /// <typeparam name="TS">The type of the status</typeparam>
    /// <typeparam name="TD">The type of the data</typeparam>
    /// <param name="value">The status result to check</param>
    /// <param name="expected">The expected status value</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void HasStatus<TS, TD>(
        this IStatusResult<TS, TD> value,
        TS expected,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (!EqualityComparer<TS>.Default.Equals(value.Status, expected))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)}.Status ({value.Status}) is not {expected}"
            );
    }

    /// <summary>
    /// Asserts that the result carries the expected data.
    /// </summary>
    /// <typeparam name="TD">The type of the result data</typeparam>
    /// <param name="value">The result to check</param>
    /// <param name="expected">The expected data value</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void HasData<TD>(
        this IResult<TD> value,
        TD expected,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (!EqualityComparer<TD>.Default.Equals(value.Data, expected))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)}.Data ({value.Data}) is not {expected}"
            );
    }

    /// <summary>
    /// Asserts that the status result carries the expected data.
    /// </summary>
    /// <typeparam name="TS">The type of the status</typeparam>
    /// <typeparam name="TD">The type of the data</typeparam>
    /// <param name="value">The status result to check</param>
    /// <param name="expected">The expected data value</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void HasData<TS, TD>(
        this IStatusResult<TS, TD> value,
        TD expected,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (!EqualityComparer<TD>.Default.Equals(value.Data, expected))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)}.Data ({value.Data}) is not {expected}"
            );
    }

    /// <summary>
    /// Asserts that the boolean result carries the expected data.
    /// </summary>
    /// <typeparam name="TD">The type of the result data</typeparam>
    /// <param name="value">The boolean result to check</param>
    /// <param name="expected">The expected data value</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
    public static void HasSuccessData<TD>(
        this IBooleanResult<TD> value,
        TD expected,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (!EqualityComparer<TD>.Default.Equals(value.Data, expected))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)}.Data ({value.Data}) is not {expected}"
            );
    }
}
