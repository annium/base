using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Operations;

public static class TaskExtensions
{
    public static async Task<T> GetData<T>(this Task<IResult<T>> task)
    {
        var result = await task;

        return result.Data;
    }

    public static async Task<IReadOnlyCollection<T>> GetData<T>(this Task<IResult<IEnumerable<T>>> task)
    {
        var response = await task;

        return response.Data.ToArray();
    }

    public static async Task<bool> GetStatus(this Task<IBooleanResult> task)
    {
        var result = await task;

        return result.IsSuccess;
    }

    public static async Task<bool> GetStatus<T>(this Task<IBooleanResult<T>> task)
    {
        var result = await task;

        return result.IsSuccess;
    }

    public static async Task<T> GetData<T>(this Task<IBooleanResult<T>> task)
    {
        var result = await task;

        return result.Data;
    }

    public static async Task<IReadOnlyCollection<T>> GetData<T>(this Task<IBooleanResult<IEnumerable<T>>> task)
    {
        var result = await task;

        return result.Data.ToArray();
    }

    public static async Task<TS> GetStatus<TS>(this Task<IStatusResult<TS>> task)
    {
        var result = await task;

        return result.Status;
    }

    public static async Task<TS> GetStatus<TS, TD>(this Task<IStatusResult<TS, TD>> task)
    {
        var result = await task;

        return result.Status;
    }

    public static async Task<TD> GetData<TS, TD>(this Task<IStatusResult<TS, TD>> task)
    {
        var result = await task;

        return result.Data;
    }

    public static async Task<IReadOnlyCollection<TD>> GetData<TS, TD>(this Task<IStatusResult<TS, IEnumerable<TD>>> task)
    {
        var result = await task;

        return result.Data.ToArray();
    }
}