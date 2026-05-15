using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium;

/// <summary>
/// Represents a box that manages asynchronous disposable resources and provides thread-safe operations for adding and removing them.
/// </summary>
/// <remarks>
/// <b>Drain ordering on <see cref="DisposeAsync"/>:</b> synchronous resources are drained FIRST (low-level
/// dependencies), then asynchronous resources in parallel. Callers MUST register resources in dependency order:
/// async resources that depend on a sync resource must NOT assume the sync resource is still alive during their
/// own teardown. This is the inverse of typical "high-level first" expectations and is encoded here because
/// the box's primary use is wiring application-level (async) consumers over framework-level (sync) primitives.
/// </remarks>
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
    /// <remarks>
    /// Drain order: synchronous resources first (via <see cref="DisposableBoxBase{TBox}.DisposeBase"/>),
    /// then asynchronous resources in parallel via <see cref="Task.WhenAll(System.Collections.Generic.IEnumerable{Task})"/>.
    /// Callers MUST NOT register asynchronous resources that depend on synchronous ones still being alive
    /// during their own teardown — those will observe the dependency already disposed.
    /// </remarks>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        DisposeBase();

        // Snapshot both queues before composing so the lock-protected Pulls happen up front,
        // then run all teardowns in a single WhenAll. A single WhenAll guarantees that a
        // throwing task in either queue does NOT skip teardown of the other queue — every
        // registered dispose runs and exceptions aggregate into one AggregateException.
        var asyncDisposableTasks = Pull(_asyncDisposables)
            .Select(async entry =>
            {
                this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                await entry.DisposeAsync().ConfigureAwait(false);
                this.Trace<string>("dispose {entry} - done", entry.GetFullId());
            });
        var asyncDisposeTasks = Pull(_asyncDisposes)
            .Select(async entry =>
            {
                this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                await entry().ConfigureAwait(false);
                this.Trace<string>("dispose {entry} - done", entry.GetFullId());
            });
        await Task.WhenAll(asyncDisposableTasks.Concat(asyncDisposeTasks)).ConfigureAwait(false);

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

    /// <summary>Adds an asynchronous <see cref="IAsyncDisposable"/> to the box's async-disposables list.</summary>
    /// <param name="disposable">The async disposable to add.</param>
    /// <returns>This box instance.</returns>
    private AsyncDisposableBox AddAsyncDisposable(IAsyncDisposable disposable) => Add(_asyncDisposables, disposable);

    /// <summary>Adds a collection of asynchronous <see cref="IAsyncDisposable"/>s to the box's async-disposables list.</summary>
    /// <param name="disposables">The async disposables to add.</param>
    /// <returns>This box instance.</returns>
    private AsyncDisposableBox AddAsyncDisposables(IEnumerable<IAsyncDisposable> disposables) =>
        Add(_asyncDisposables, disposables);

    /// <summary>Removes an asynchronous <see cref="IAsyncDisposable"/> from the box's async-disposables list.</summary>
    /// <param name="disposable">The async disposable to remove.</param>
    /// <returns>This box instance.</returns>
    private AsyncDisposableBox RemoveAsyncDisposable(IAsyncDisposable disposable) =>
        Remove(_asyncDisposables, disposable);

    /// <summary>Removes a collection of asynchronous <see cref="IAsyncDisposable"/>s from the box's async-disposables list.</summary>
    /// <param name="disposables">The async disposables to remove.</param>
    /// <returns>This box instance.</returns>
    private AsyncDisposableBox RemoveAsyncDisposables(IEnumerable<IAsyncDisposable> disposables) =>
        Remove(_asyncDisposables, disposables);

    /// <summary>Adds an asynchronous dispose function to the box's async-disposes list.</summary>
    /// <param name="dispose">The async dispose function to add.</param>
    /// <returns>This box instance.</returns>
    private AsyncDisposableBox AddAsyncDispose(Func<Task> dispose) => Add(_asyncDisposes, dispose);

    /// <summary>Adds a collection of asynchronous dispose functions to the box's async-disposes list.</summary>
    /// <param name="disposes">The async dispose functions to add.</param>
    /// <returns>This box instance.</returns>
    private AsyncDisposableBox AddAsyncDisposes(IEnumerable<Func<Task>> disposes) => Add(_asyncDisposes, disposes);

    /// <summary>Removes an asynchronous dispose function from the box's async-disposes list.</summary>
    /// <param name="dispose">The async dispose function to remove.</param>
    /// <returns>This box instance.</returns>
    private AsyncDisposableBox RemoveAsyncDispose(Func<Task> dispose) => Remove(_asyncDisposes, dispose);

    /// <summary>Removes a collection of asynchronous dispose functions from the box's async-disposes list.</summary>
    /// <param name="disposes">The async dispose functions to remove.</param>
    /// <returns>This box instance.</returns>
    private AsyncDisposableBox RemoveAsyncDisposes(IEnumerable<Func<Task>> disposes) =>
        Remove(_asyncDisposes, disposes);

    /// <summary>
    /// Adds a synchronous disposable resource to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IDisposable disposable) =>
        box.AddSyncDisposable(disposable);

    /// <summary>
    /// Removes a synchronous disposable resource from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IDisposable disposable) =>
        box.RemoveSyncDisposable(disposable);

    /// <summary>
    /// Adds a collection of synchronous disposable resources to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.AddSyncDisposables(disposables);

    /// <summary>
    /// Removes a collection of synchronous disposable resources from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.RemoveSyncDisposables(disposables);

    /// <summary>
    /// Adds an asynchronous disposable resource to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IAsyncDisposable disposable) =>
        box.AddAsyncDisposable(disposable);

    /// <summary>
    /// Removes an asynchronous disposable resource from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IAsyncDisposable disposable) =>
        box.RemoveAsyncDisposable(disposable);

    /// <summary>
    /// Adds a collection of asynchronous disposable resources to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) =>
        box.AddAsyncDisposables(disposables);

    /// <summary>
    /// Removes a collection of asynchronous disposable resources from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) =>
        box.RemoveAsyncDisposables(disposables);

    /// <summary>
    /// Adds a synchronous dispose action to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, Action dispose) => box.AddSyncDispose(dispose);

    /// <summary>
    /// Removes a synchronous dispose action from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, Action dispose) =>
        box.RemoveSyncDispose(dispose);

    /// <summary>
    /// Adds a collection of synchronous dispose actions to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Action> disposes) =>
        box.AddSyncDisposes(disposes);

    /// <summary>
    /// Removes a collection of synchronous dispose actions from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Action> disposes) =>
        box.RemoveSyncDisposes(disposes);

    /// <summary>
    /// Adds an asynchronous dispose function to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, Func<Task> dispose) =>
        box.AddAsyncDispose(dispose);

    /// <summary>
    /// Removes an asynchronous dispose function from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, Func<Task> dispose) =>
        box.RemoveAsyncDispose(dispose);

    /// <summary>
    /// Adds a collection of asynchronous dispose functions to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) =>
        box.AddAsyncDisposes(disposes);

    /// <summary>
    /// Removes a collection of asynchronous dispose functions from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) =>
        box.RemoveAsyncDisposes(disposes);
}
