using System;
using System.ComponentModel;
using System.Threading;

namespace Annium.Threading;

public static class CancellationTokenExtensions
{
    /// <summary>
    /// Allows a cancellation token to be awaited.
    /// </summary>
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
    internal struct CancellationTokenAwaiter : IAwaiter
    {
        private readonly CancellationToken _cancellationToken;

        public CancellationTokenAwaiter(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public void GetResult()
        {
            // this is called by compiler generated methods when the
            // task has completed.
            if (!IsCompleted)
                throw new InvalidOperationException("The cancellation token has not yet been cancelled.");
        }

        // called by compiler generated/.NET internals to check
        // if the task has completed.
        public bool IsCompleted => _cancellationToken.IsCancellationRequested;

        // The compiler will generate stuff that hooks in
        // here. We hook those methods directly into the
        // cancellation token.
        public void OnCompleted(Action continuation) =>
            _cancellationToken.Register(continuation);

        public void UnsafeOnCompleted(Action continuation) =>
            _cancellationToken.Register(continuation);
    }
}