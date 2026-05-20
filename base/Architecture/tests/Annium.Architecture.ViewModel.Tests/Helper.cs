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
    public int Invocations { get; private set; }

    public bool HasMap<T>(object? source) => true;

    public bool HasMap(object? source, Type? type) => true;

    public T Map<T>(object? source)
    {
        Invocations++;
        return default!;
    }

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
    private readonly object _result;

    public StubMapper(object result)
    {
        _result = result;
    }

    public int Invocations { get; private set; }

    public bool HasMap<T>(object? source) => true;

    public bool HasMap(object? source, Type? type) => true;

    public T Map<T>(object? source)
    {
        Invocations++;
        return (T)_result;
    }

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
    public void Log(
        object subject,
        string file,
        string member,
        int line,
        LogLevel level,
        string message,
        IReadOnlyList<object?> data
    ) { }

    public void Error(
        object subject,
        string file,
        string member,
        int line,
        Exception ex,
        IReadOnlyList<object?> data
    ) { }
}
