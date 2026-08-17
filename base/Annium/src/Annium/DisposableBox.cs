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
    /// <param name="box">The box to add the resource to.</param>
    /// <param name="disposable">The disposable resource to add.</param>
    /// <returns>The same box instance, so operators can be chained.</returns>
    public static DisposableBox operator +(DisposableBox box, IDisposable disposable) =>
        box.AddSyncDisposable(disposable);

    /// <summary>
    /// Removes a disposable resource from the box.
    /// </summary>
    /// <param name="box">The box to remove the resource from.</param>
    /// <param name="disposable">The disposable resource to remove.</param>
    /// <returns>The same box instance, so operators can be chained.</returns>
    public static DisposableBox operator -(DisposableBox box, IDisposable disposable) =>
        box.RemoveSyncDisposable(disposable);

    /// <summary>
    /// Adds a collection of disposable resources to the box.
    /// </summary>
    /// <param name="box">The box to add the resources to.</param>
    /// <param name="disposables">The disposable resources to add.</param>
    /// <returns>The same box instance, so operators can be chained.</returns>
    public static DisposableBox operator +(DisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.AddSyncDisposables(disposables);

    /// <summary>
    /// Removes a collection of disposable resources from the box.
    /// </summary>
    /// <param name="box">The box to remove the resources from.</param>
    /// <param name="disposables">The disposable resources to remove.</param>
    /// <returns>The same box instance, so operators can be chained.</returns>
    public static DisposableBox operator -(DisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.RemoveSyncDisposables(disposables);

    /// <summary>
    /// Adds a dispose action to the box.
    /// </summary>
    /// <param name="box">The box to add the action to.</param>
    /// <param name="dispose">The dispose action to add.</param>
    /// <returns>The same box instance, so operators can be chained.</returns>
    public static DisposableBox operator +(DisposableBox box, Action dispose) => box.AddSyncDispose(dispose);

    /// <summary>
    /// Removes a dispose action from the box.
    /// </summary>
    /// <param name="box">The box to remove the action from.</param>
    /// <param name="dispose">The dispose action to remove.</param>
    /// <returns>The same box instance, so operators can be chained.</returns>
    public static DisposableBox operator -(DisposableBox box, Action dispose) => box.RemoveSyncDispose(dispose);

    /// <summary>
    /// Adds a collection of dispose actions to the box.
    /// </summary>
    /// <param name="box">The box to add the actions to.</param>
    /// <param name="disposes">The dispose actions to add.</param>
    /// <returns>The same box instance, so operators can be chained.</returns>
    public static DisposableBox operator +(DisposableBox box, IEnumerable<Action> disposes) =>
        box.AddSyncDisposes(disposes);

    /// <summary>
    /// Removes a collection of dispose actions from the box.
    /// </summary>
    /// <param name="box">The box to remove the actions from.</param>
    /// <param name="disposes">The dispose actions to remove.</param>
    /// <returns>The same box instance, so operators can be chained.</returns>
    public static DisposableBox operator -(DisposableBox box, IEnumerable<Action> disposes) =>
        box.RemoveSyncDisposes(disposes);
}
