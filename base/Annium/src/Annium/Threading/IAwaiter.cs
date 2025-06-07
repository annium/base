using System.Runtime.CompilerServices;

namespace Annium.Threading;

/// <summary>
/// Represents an awaiter that can be used to await an asynchronous operation and get its result.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public interface IAwaiter<T> : ICriticalNotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether the awaited operation has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the awaited operation.
    /// </summary>
    /// <returns>The result of the awaited operation.</returns>
    T GetResult();
}

/// <summary>
/// Represents an awaiter that can be used to await an asynchronous operation.
/// </summary>
public interface IAwaiter : ICriticalNotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether the awaited operation has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the awaited operation.
    /// </summary>
    void GetResult();
}
