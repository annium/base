using System;
using System.Threading;

namespace Annium;

/// <summary>
/// Represents a thread-safe container for a value that can be initialized once.
/// </summary>
/// <typeparam name="T">The type of the value to be injected.</typeparam>
public sealed record Injected<T>
{
    /// <summary>
    /// Gets the injected value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value has not been initialized.</exception>
    public T Value
    {
        get
        {
            if (_isInitialized == 0)
                throw new InvalidOperationException($"{typeof(T).FriendlyName()} is not initialized");

            return field;
        }
        private set;
    } = default!;

    /// <summary>
    /// A flag indicating whether the value has been initialized (1) or not (0).
    /// </summary>
    private int _isInitialized;

    /// <summary>
    /// Initializes the container with the specified value.
    /// </summary>
    /// <param name="value">The value to inject.</param>
    /// <exception cref="InvalidOperationException">Thrown when the value has already been initialized.</exception>
    public void Init(T value)
    {
        if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) != 0)
            throw new InvalidOperationException($"{typeof(T).FriendlyName()} is already initialized");

        Value = value;
    }
}
