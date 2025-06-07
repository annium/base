using System;

namespace Annium;

/// <summary>
/// Represents a reference to a value that can be disposed asynchronously.
/// </summary>
/// <typeparam name="TValue">The type of the value being referenced.</typeparam>
public interface IDisposableReference<out TValue> : IAsyncDisposable
    where TValue : notnull
{
    /// <summary>
    /// Gets the value being referenced.
    /// </summary>
    TValue Value { get; }
}
