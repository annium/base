using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Operations;

public static class TaskExtensions
{
    public static async Task<T> GetDataAsync<T>(this Task<IResult<T>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Data;
    }

    public static async Task<IReadOnlyCollection<T>> GetDataAsync<T>(this Task<IResult<IEnumerable<T>>> task)
    {
#pragma warning disable VSTHRD003
        var response = await task;
#pragma warning restore VSTHRD003

        return response.Data.ToArray();
    }

    public static async Task<bool> GetStatusAsync(this Task<IBooleanResult> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.IsSuccess;
    }

    public static async Task<bool> GetStatusAsync<T>(this Task<IBooleanResult<T>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.IsSuccess;
    }

    public static async Task<T> GetDataAsync<T>(this Task<IBooleanResult<T>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Data;
    }

    public static async Task<IReadOnlyCollection<T>> GetDataAsync<T>(this Task<IBooleanResult<IEnumerable<T>>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Data.ToArray();
    }

    public static async Task<TS> GetStatusAsync<TS>(this Task<IStatusResult<TS>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Status;
    }

    public static async Task<TS> GetStatusAsync<TS, TD>(this Task<IStatusResult<TS, TD>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Status;
    }

    public static async Task<TD> GetDataAsync<TS, TD>(this Task<IStatusResult<TS, TD>> task)
    {
#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003

        return result.Data;
    }

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
