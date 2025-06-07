using System.Threading.Tasks;

namespace Annium.Threading.Tasks;

/// <summary>
/// Provides extension methods for awaiting tasks synchronously.
/// </summary>
public static class AwaitableExtensions
{
#pragma warning disable VSTHRD002
    /// <summary>
    /// Awaits a task synchronously.
    /// </summary>
    /// <param name="task">The task to await.</param>
    public static void Await(this Task task) => task.GetAwaiter().GetResult();

    /// <summary>
    /// Awaits a task synchronously and returns its result.
    /// </summary>
    /// <typeparam name="T">The type of the task's result.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <returns>The result of the task.</returns>
    public static T Await<T>(this Task<T> task) => task.GetAwaiter().GetResult();

    /// <summary>
    /// Awaits a value task synchronously.
    /// </summary>
    /// <param name="task">The value task to await.</param>
    public static void Await(this ValueTask task) => task.GetAwaiter().GetResult();

    /// <summary>
    /// Awaits a value task synchronously and returns its result.
    /// </summary>
    /// <typeparam name="T">The type of the value task's result.</typeparam>
    /// <param name="task">The value task to await.</param>
    /// <returns>The result of the value task.</returns>
    public static T Await<T>(this ValueTask<T> task) => task.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
}
