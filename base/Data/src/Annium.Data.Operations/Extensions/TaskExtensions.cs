using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Operations;

/// <summary>
/// Extension methods for Task types containing result objects.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Gets the data from a task containing an IResult.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="task">The task containing the result.</param>
    /// <returns>The data from the result.</returns>
    public static async Task<T> GetDataAsync<T>(this Task<IResult<T>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Data;
    }

    /// <summary>
    /// Gets the data as a read-only collection from a task containing an IResult with enumerable data.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="task">The task containing the result.</param>
    /// <returns>The data as a read-only collection.</returns>
    public static async Task<IReadOnlyCollection<T>> GetDataAsync<T>(this Task<IResult<IEnumerable<T>>> task)
    {
#pragma warning disable VSTHRD003
        var response = await task;
#pragma warning restore VSTHRD003

        return response.Data.ToArray();
    }

    /// <summary>
    /// Gets the success status from a task containing an IBooleanResult.
    /// </summary>
    /// <param name="task">The task containing the boolean result.</param>
    /// <returns>True if the result is successful, false otherwise.</returns>
    public static async Task<bool> GetStatusAsync(this Task<IBooleanResult> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.IsSuccess;
    }

    /// <summary>
    /// Gets the success status from a task containing an IBooleanResult with data.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="task">The task containing the boolean result.</param>
    /// <returns>True if the result is successful, false otherwise.</returns>
    public static async Task<bool> GetStatusAsync<T>(this Task<IBooleanResult<T>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.IsSuccess;
    }

    /// <summary>
    /// Gets the data from a task containing an IBooleanResult with data.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="task">The task containing the boolean result.</param>
    /// <returns>The data from the result.</returns>
    public static async Task<T> GetDataAsync<T>(this Task<IBooleanResult<T>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Data;
    }

    /// <summary>
    /// Gets the data as a read-only collection from a task containing an IBooleanResult with enumerable data.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="task">The task containing the boolean result.</param>
    /// <returns>The data as a read-only collection.</returns>
    public static async Task<IReadOnlyCollection<T>> GetDataAsync<T>(this Task<IBooleanResult<IEnumerable<T>>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Data.ToArray();
    }

    /// <summary>
    /// Gets the status from a task containing an IStatusResult.
    /// </summary>
    /// <typeparam name="TS">The status type.</typeparam>
    /// <param name="task">The task containing the status result.</param>
    /// <returns>The status from the result.</returns>
    public static async Task<TS> GetStatusAsync<TS>(this Task<IStatusResult<TS>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Status;
    }

    /// <summary>
    /// Gets the status from a task containing an IStatusResult with data.
    /// </summary>
    /// <typeparam name="TS">The status type.</typeparam>
    /// <typeparam name="TD">The data type.</typeparam>
    /// <param name="task">The task containing the status result.</param>
    /// <returns>The status from the result.</returns>
    public static async Task<TS> GetStatusAsync<TS, TD>(this Task<IStatusResult<TS, TD>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Status;
    }

    /// <summary>
    /// Gets the data from a task containing an IStatusResult with data.
    /// </summary>
    /// <typeparam name="TS">The status type.</typeparam>
    /// <typeparam name="TD">The data type.</typeparam>
    /// <param name="task">The task containing the status result.</param>
    /// <returns>The data from the result.</returns>
    public static async Task<TD> GetDataAsync<TS, TD>(this Task<IStatusResult<TS, TD>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Data;
    }

    /// <summary>
    /// Gets the data as a read-only collection from a task containing an IStatusResult with enumerable data.
    /// </summary>
    /// <typeparam name="TS">The status type.</typeparam>
    /// <typeparam name="TD">The element type.</typeparam>
    /// <param name="task">The task containing the status result.</param>
    /// <returns>The data as a read-only collection.</returns>
    public static async Task<IReadOnlyCollection<TD>> GetDataAsync<TS, TD>(
        this Task<IStatusResult<TS, IEnumerable<TD>>> task
    )
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Data.ToArray();
    }
}
