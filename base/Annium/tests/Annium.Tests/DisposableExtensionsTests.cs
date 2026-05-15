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
        var task = ((IDisposable)disposable).DisposeAsync();
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

    private sealed class SyncDisposable : IDisposable
    {
        public bool Disposed { get; private set; }
        public bool AsyncDisposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    private sealed class DualDisposable : IDisposable, IAsyncDisposable
    {
        public bool Disposed { get; private set; }
        public bool AsyncDisposed { get; private set; }

        public void Dispose() => Disposed = true;

        public ValueTask DisposeAsync()
        {
            AsyncDisposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
