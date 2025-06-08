using System;
using System.ComponentModel;
using System.Threading;

namespace Annium.Threading;

/// <summary>
/// Provides extension methods for working with cancellation tokens.
/// </summary>
public static class CancellationTokenExtensions
{
    /// <summary>
    /// Allows a cancellation token to be awaited.
    /// </summary>
    /// <param name="ct">The cancellation token to await.</param>
    /// <returns>An awaiter for the cancellation token.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IAwaiter GetAwaiter(this CancellationToken ct)
    {
        // return our special awaiter
        return new CancellationTokenAwaiter(ct);
    }

    /// <summary>
    /// The awaiter for cancellation tokens.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal readonly struct CancellationTokenAwaiter : IAwaiter
    {
        /// <summary>
        /// The cancellation token being awaited.
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationTokenAwaiter"/> struct.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to await.</param>
        public CancellationTokenAwaiter(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the result of the awaited operation.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the cancellation token has not been cancelled.</exception>
        public void GetResult()
        {
            // this is called by compiler generated methods when the
            // task has completed.
            if (!IsCompleted)
                throw new InvalidOperationException("The cancellation token has not yet been cancelled.");
        }

        /// <summary>
        /// Gets a value indicating whether the awaited operation has completed.
        /// </summary>
        /// <returns>True if the cancellation token has been cancelled; otherwise, false.</returns>
        public bool IsCompleted => _cancellationToken.IsCancellationRequested;

        /// <summary>
        /// Schedules the continuation action that's invoked when the instance completes.
        /// </summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        public void OnCompleted(Action continuation) => _cancellationToken.Register(continuation);

        /// <summary>
        /// Schedules the continuation action that's invoked when the instance completes.
        /// </summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        public void UnsafeOnCompleted(Action continuation) => _cancellationToken.Register(continuation);
    }
}
