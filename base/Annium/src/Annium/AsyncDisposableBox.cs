using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium;

/// <summary>
/// Represents a box that manages asynchronous disposable resources and provides thread-safe operations for adding and removing them.
/// </summary>
public sealed class AsyncDisposableBox : DisposableBoxBase<AsyncDisposableBox>, IAsyncDisposable
{
    /// <summary>
    /// A list of asynchronous disposable resources managed by this box.
    /// </summary>
    private readonly List<IAsyncDisposable> _asyncDisposables = new();

    /// <summary>
    /// A list of asynchronous dispose functions managed by this box.
    /// </summary>
    private readonly List<Func<Task>> _asyncDisposes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncDisposableBox"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    internal AsyncDisposableBox(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Disposes all resources and resets the box to its initial state.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask DisposeAndResetAsync()
    {
        await DisposeAsync().ConfigureAwait(false);
        Reset();
    }

    /// <summary>
    /// Asynchronously disposes all resources in the box.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        DisposeBase();

        if (_asyncDisposables.Count > 0)
            await Task.WhenAll(
                    Pull(_asyncDisposables)
                        .Select(async entry =>
                        {
                            this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                            await entry.DisposeAsync().ConfigureAwait(false);
                            this.Trace<string>("dispose {entry} - done", entry.GetFullId());
                        })
                )
                .ConfigureAwait(false);
        if (_asyncDisposes.Count > 0)
            await Task.WhenAll(
                    Pull(_asyncDisposes)
                        .Select(async entry =>
                        {
                            this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                            await entry().ConfigureAwait(false);
                            this.Trace<string>("dispose {entry} - done", entry.GetFullId());
                        })
                )
                .ConfigureAwait(false);

        this.Trace("done");
    }

    /// <summary>
    /// Clears the async disposable and async dispose lists when the box is reset. Invoked under the
    /// base class lock so the reset is atomic with the sync-list clear.
    /// </summary>
    protected override void ResetCore()
    {
        _asyncDisposables.Clear();
        _asyncDisposes.Clear();
    }

    /// <summary>
    /// Adds a synchronous disposable resource to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IDisposable disposable) =>
        box.Add(box.SyncDisposables, disposable);

    /// <summary>
    /// Removes a synchronous disposable resource from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IDisposable disposable) =>
        box.Remove(box.SyncDisposables, disposable);

    /// <summary>
    /// Adds a collection of synchronous disposable resources to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.Add(box.SyncDisposables, disposables);

    /// <summary>
    /// Removes a collection of synchronous disposable resources from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.Remove(box.SyncDisposables, disposables);

    /// <summary>
    /// Adds an asynchronous disposable resource to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IAsyncDisposable disposable) =>
        box.Add(box._asyncDisposables, disposable);

    /// <summary>
    /// Removes an asynchronous disposable resource from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IAsyncDisposable disposable) =>
        box.Remove(box._asyncDisposables, disposable);

    /// <summary>
    /// Adds a collection of asynchronous disposable resources to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) =>
        box.Add(box._asyncDisposables, disposables);

    /// <summary>
    /// Removes a collection of asynchronous disposable resources from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) =>
        box.Remove(box._asyncDisposables, disposables);

    /// <summary>
    /// Adds a synchronous dispose action to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, Action dispose) =>
        box.Add(box.SyncDisposes, dispose);

    /// <summary>
    /// Removes a synchronous dispose action from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, Action dispose) =>
        box.Remove(box.SyncDisposes, dispose);

    /// <summary>
    /// Adds a collection of synchronous dispose actions to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Action> disposes) =>
        box.Add(box.SyncDisposes, disposes);

    /// <summary>
    /// Removes a collection of synchronous dispose actions from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Action> disposes) =>
        box.Remove(box.SyncDisposes, disposes);

    /// <summary>
    /// Adds an asynchronous dispose function to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, Func<Task> dispose) =>
        box.Add(box._asyncDisposes, dispose);

    /// <summary>
    /// Removes an asynchronous dispose function from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, Func<Task> dispose) =>
        box.Remove(box._asyncDisposes, dispose);

    /// <summary>
    /// Adds a collection of asynchronous dispose functions to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) =>
        box.Add(box._asyncDisposes, disposes);

    /// <summary>
    /// Removes a collection of asynchronous dispose functions from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) =>
        box.Remove(box._asyncDisposes, disposes);
}
