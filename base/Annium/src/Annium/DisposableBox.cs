using System;
using System.Collections.Generic;
using Annium.Logging;

namespace Annium;

/// <summary>
/// Represents a box that manages synchronous disposable resources and provides thread-safe operations for adding and removing them.
/// </summary>
public sealed class DisposableBox : DisposableBoxBase<DisposableBox>, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableBox"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    internal DisposableBox(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Disposes all resources and resets the box to its initial state.
    /// </summary>
    public void DisposeAndReset()
    {
        Dispose();
        Reset();
    }

    /// <summary>
    /// Disposes all resources in the box.
    /// </summary>
    public void Dispose()
    {
        this.Trace("start");
        DisposeBase();
        this.Trace("done");
    }

    /// <summary>
    /// Adds a disposable resource to the box.
    /// </summary>
    public static DisposableBox operator +(DisposableBox box, IDisposable disposable) =>
        box.Add(box.SyncDisposables, disposable);

    /// <summary>
    /// Removes a disposable resource from the box.
    /// </summary>
    public static DisposableBox operator -(DisposableBox box, IDisposable disposable) =>
        box.Remove(box.SyncDisposables, disposable);

    /// <summary>
    /// Adds a collection of disposable resources to the box.
    /// </summary>
    public static DisposableBox operator +(DisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.Add(box.SyncDisposables, disposables);

    /// <summary>
    /// Removes a collection of disposable resources from the box.
    /// </summary>
    public static DisposableBox operator -(DisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.Remove(box.SyncDisposables, disposables);

    /// <summary>
    /// Adds a dispose action to the box.
    /// </summary>
    public static DisposableBox operator +(DisposableBox box, Action dispose) => box.Add(box.SyncDisposes, dispose);

    /// <summary>
    /// Removes a dispose action from the box.
    /// </summary>
    public static DisposableBox operator -(DisposableBox box, Action dispose) => box.Remove(box.SyncDisposes, dispose);

    /// <summary>
    /// Adds a collection of dispose actions to the box.
    /// </summary>
    public static DisposableBox operator +(DisposableBox box, IEnumerable<Action> disposes) =>
        box.Add(box.SyncDisposes, disposes);

    /// <summary>
    /// Removes a collection of dispose actions from the box.
    /// </summary>
    public static DisposableBox operator -(DisposableBox box, IEnumerable<Action> disposes) =>
        box.Remove(box.SyncDisposes, disposes);
}
