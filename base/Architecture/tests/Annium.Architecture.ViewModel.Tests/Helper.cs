using System;
using System.Collections.Generic;
using Annium.Core.Mapper;
using Annium.Logging;

namespace Annium.Architecture.ViewModel.Tests;

/// <summary>
/// Mapper that records invocations without producing a real result. Used to prove
/// non-Ok / skip paths bypass the mapper entirely via an independent
/// <c>Invocations.Is(0)</c> assertion (rather than relying on a thrown exception
/// as the regression signal).
/// </summary>
internal sealed class RecordingMapper : IMapper
{
    /// <summary>Number of times <c>Map</c> has been called on this instance.</summary>
    public int Invocations { get; private set; }

    /// <summary>
    /// Always returns <see langword="true"/> — this mapper claims to support all source types.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="source">The source object (ignored).</param>
    /// <returns><see langword="true"/> unconditionally.</returns>
    public bool HasMap<T>(object? source) => true;

    /// <summary>
    /// Always returns <see langword="true"/> — this mapper claims to support all source/destination combinations.
    /// </summary>
    /// <param name="source">The source object (ignored).</param>
    /// <param name="type">The destination type (ignored).</param>
    /// <returns><see langword="true"/> unconditionally.</returns>
    public bool HasMap(object? source, Type? type) => true;

    /// <summary>
    /// Increments the invocation counter and returns the default value for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="source">The source object (ignored).</param>
    /// <returns>The default value of <typeparamref name="T"/>.</returns>
    public T Map<T>(object? source)
    {
        Invocations++;
        return default!;
    }

    /// <summary>
    /// Increments the invocation counter and returns <see langword="null"/>.
    /// </summary>
    /// <param name="source">The source object (ignored).</param>
    /// <param name="type">The destination type (ignored).</param>
    /// <returns><see langword="null"/>.</returns>
    public object? Map(object? source, Type type)
    {
        Invocations++;
        return null;
    }
}

/// <summary>
/// Mapper that returns a fixed instance — used to prove the Ok path does invoke mapping.
/// </summary>
internal sealed class StubMapper : IMapper
{
    /// <summary>The fixed instance returned by every <c>Map</c> call.</summary>
    private readonly object _result;

    public StubMapper(object result)
    {
        _result = result;
    }

    /// <summary>Number of times <c>Map</c> has been called on this instance.</summary>
    public int Invocations { get; private set; }

    /// <summary>
    /// Always returns <see langword="true"/> — this mapper claims to support all source types.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="source">The source object (ignored).</param>
    /// <returns><see langword="true"/> unconditionally.</returns>
    public bool HasMap<T>(object? source) => true;

    /// <summary>
    /// Always returns <see langword="true"/> — this mapper claims to support all source/destination combinations.
    /// </summary>
    /// <param name="source">The source object (ignored).</param>
    /// <param name="type">The destination type (ignored).</param>
    /// <returns><see langword="true"/> unconditionally.</returns>
    public bool HasMap(object? source, Type? type) => true;

    /// <summary>
    /// Increments the invocation counter and returns the fixed result cast to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="source">The source object (ignored).</param>
    /// <returns>The fixed result cast to <typeparamref name="T"/>.</returns>
    public T Map<T>(object? source)
    {
        Invocations++;
        return (T)_result;
    }

    /// <summary>
    /// Increments the invocation counter and returns the fixed result object.
    /// </summary>
    /// <param name="source">The source object (ignored).</param>
    /// <param name="type">The destination type (ignored).</param>
    /// <returns>The fixed result object.</returns>
    public object? Map(object? source, Type type)
    {
        Invocations++;
        return _result;
    }
}

/// <summary>
/// No-op logger sufficient for these unit tests.
/// </summary>
internal sealed class NullLogger : ILogger
{
    /// <summary>
    /// Discards the log entry — no-op implementation sufficient for unit tests that do not
    /// inspect logged output.
    /// </summary>
    /// <param name="subject">The logging subject.</param>
    /// <param name="file">Source file name.</param>
    /// <param name="member">Caller member name.</param>
    /// <param name="line">Source line number.</param>
    /// <param name="level">Log severity level.</param>
    /// <param name="message">Formatted log message.</param>
    /// <param name="data">Structured log data.</param>
    public void Log(
        object subject,
        string file,
        string member,
        int line,
        LogLevel level,
        string message,
        IReadOnlyList<object?> data
    ) { }

    /// <summary>
    /// Discards the exception log entry — no-op implementation sufficient for unit tests that do
    /// not inspect error output.
    /// </summary>
    /// <param name="subject">The logging subject.</param>
    /// <param name="file">Source file name.</param>
    /// <param name="member">Caller member name.</param>
    /// <param name="line">Source line number.</param>
    /// <param name="ex">The exception to log.</param>
    /// <param name="data">Structured log data.</param>
    public void Error(
        object subject,
        string file,
        string member,
        int line,
        Exception ex,
        IReadOnlyList<object?> data
    ) { }
}
