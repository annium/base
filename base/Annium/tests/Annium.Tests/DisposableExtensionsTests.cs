using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Tests for <see cref="DisposableExtensions"/>. Closes the TG5 zero-coverage gap on the
/// sync/async dispatch in <c>DisposeAsync(IDisposable)</c>.
/// </summary>
public class DisposableExtensionsTests
{
    /// <summary>
    /// Verifies that DisposeAsync on a plain IDisposable calls Dispose synchronously and returns a
    /// completed ValueTask.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DisposeAsync_SyncOnly_CallsDispose()
    {
        var disposable = new SyncDisposable();
        var task = disposable.DisposeAsync();
        task.IsCompleted.IsTrue();
        await task;
        disposable.Disposed.IsTrue();
        disposable.AsyncDisposed.IsFalse();
    }

    /// <summary>
    /// Verifies that DisposeAsync on a value that also implements IAsyncDisposable dispatches to the
    /// async path (does NOT call sync Dispose). A regression that calls sync Dispose on the async-aware
    /// type would be caught here.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DisposeAsync_AsyncDisposable_CallsDisposeAsync()
    {
        var disposable = new DualDisposable();
        await ((IDisposable)disposable).DisposeAsync();
        disposable.AsyncDisposed.IsTrue();
        disposable.Disposed.IsFalse();
    }

    /// <summary>
    /// Stub that implements only <see cref="IDisposable"/> (no async path); used to verify that
    /// <c>DisposeAsync(IDisposable)</c> falls back to the synchronous <c>Dispose</c> overload.
    /// </summary>
    private sealed class SyncDisposable : IDisposable
    {
        /// <summary>Gets a value indicating whether <see cref="Dispose"/> has been called.</summary>
        public bool Disposed { get; private set; }

        /// <summary>Gets a value indicating whether an async dispose path has been invoked (always <see langword="false"/> for this stub).</summary>
        public bool AsyncDisposed { get; private set; }

        /// <summary>
        /// Sets <see cref="Disposed"/> to <see langword="true"/>.
        /// </summary>
        /// <returns>Nothing — void method implementing <see cref="IDisposable.Dispose"/>.</returns>
        public void Dispose() => Disposed = true;
    }

    /// <summary>
    /// Stub that implements both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/>; used
    /// to verify that <c>DisposeAsync(IDisposable)</c> dispatches to the async path when available.
    /// </summary>
    private sealed class DualDisposable : IDisposable, IAsyncDisposable
    {
        /// <summary>Gets a value indicating whether the synchronous <see cref="Dispose"/> has been called.</summary>
        public bool Disposed { get; private set; }

        /// <summary>Gets a value indicating whether <see cref="DisposeAsync"/> has been called.</summary>
        public bool AsyncDisposed { get; private set; }

        /// <summary>
        /// Sets <see cref="Disposed"/> to <see langword="true"/> (sync path).
        /// </summary>
        /// <returns>Nothing — void method implementing <see cref="IDisposable.Dispose"/>.</returns>
        public void Dispose() => Disposed = true;

        /// <summary>
        /// Sets <see cref="AsyncDisposed"/> to <see langword="true"/> and returns a completed <see cref="ValueTask"/>.
        /// </summary>
        /// <returns>A completed <see cref="ValueTask"/>.</returns>
        public ValueTask DisposeAsync()
        {
            AsyncDisposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
