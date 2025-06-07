using System.Threading.Tasks;

namespace Annium.Threading.Tasks;

#pragma warning disable VSTHRD200
#pragma warning disable VSTHRD103
#pragma warning disable VSTHRD003
/// <summary>
/// Provides methods for working with sets of tasks.
/// </summary>
public static class TaskSet
{
    /// <summary>
    /// Waits for all tasks to complete and returns their results as a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first task's result.</typeparam>
    /// <typeparam name="T2">The type of the second task's result.</typeparam>
    /// <param name="t1">The first task.</param>
    /// <param name="t2">The second task.</param>
    /// <returns>A tuple containing the results of both tasks.</returns>
    public static async Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> t1, Task<T2> t2)
    {
        await Task.WhenAll(t1, t2);

        return (t1.Result, t2.Result);
    }

    /// <summary>
    /// Waits for all tasks to complete and returns their results as a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first task's result.</typeparam>
    /// <typeparam name="T2">The type of the second task's result.</typeparam>
    /// <typeparam name="T3">The type of the third task's result.</typeparam>
    /// <param name="t1">The first task.</param>
    /// <param name="t2">The second task.</param>
    /// <param name="t3">The third task.</param>
    /// <returns>A tuple containing the results of all tasks.</returns>
    public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> t1, Task<T2> t2, Task<T3> t3)
    {
        await Task.WhenAll(t1, t2, t3);

        return (t1.Result, t2.Result, t3.Result);
    }

    /// <summary>
    /// Waits for all tasks to complete and returns their results as a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first task's result.</typeparam>
    /// <typeparam name="T2">The type of the second task's result.</typeparam>
    /// <typeparam name="T3">The type of the third task's result.</typeparam>
    /// <typeparam name="T4">The type of the fourth task's result.</typeparam>
    /// <param name="t1">The first task.</param>
    /// <param name="t2">The second task.</param>
    /// <param name="t3">The third task.</param>
    /// <param name="t4">The fourth task.</param>
    /// <returns>A tuple containing the results of all tasks.</returns>
    public static async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(
        Task<T1> t1,
        Task<T2> t2,
        Task<T3> t3,
        Task<T4> t4
    )
    {
        await Task.WhenAll(t1, t2, t3, t4);

        return (t1.Result, t2.Result, t3.Result, t4.Result);
    }

    /// <summary>
    /// Waits for all tasks to complete and returns their results as a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first task's result.</typeparam>
    /// <typeparam name="T2">The type of the second task's result.</typeparam>
    /// <typeparam name="T3">The type of the third task's result.</typeparam>
    /// <typeparam name="T4">The type of the fourth task's result.</typeparam>
    /// <typeparam name="T5">The type of the fifth task's result.</typeparam>
    /// <param name="t1">The first task.</param>
    /// <param name="t2">The second task.</param>
    /// <param name="t3">The third task.</param>
    /// <param name="t4">The fourth task.</param>
    /// <param name="t5">The fifth task.</param>
    /// <returns>A tuple containing the results of all tasks.</returns>
    public static async Task<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(
        Task<T1> t1,
        Task<T2> t2,
        Task<T3> t3,
        Task<T4> t4,
        Task<T5> t5
    )
    {
        await Task.WhenAll(t1, t2, t3, t4, t5);

        return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result);
    }

    /// <summary>
    /// Waits for all tasks to complete and returns their results as a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first task's result.</typeparam>
    /// <typeparam name="T2">The type of the second task's result.</typeparam>
    /// <typeparam name="T3">The type of the third task's result.</typeparam>
    /// <typeparam name="T4">The type of the fourth task's result.</typeparam>
    /// <typeparam name="T5">The type of the fifth task's result.</typeparam>
    /// <typeparam name="T6">The type of the sixth task's result.</typeparam>
    /// <param name="t1">The first task.</param>
    /// <param name="t2">The second task.</param>
    /// <param name="t3">The third task.</param>
    /// <param name="t4">The fourth task.</param>
    /// <param name="t5">The fifth task.</param>
    /// <param name="t6">The sixth task.</param>
    /// <returns>A tuple containing the results of all tasks.</returns>
    public static async Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(
        Task<T1> t1,
        Task<T2> t2,
        Task<T3> t3,
        Task<T4> t4,
        Task<T5> t5,
        Task<T6> t6
    )
    {
        await Task.WhenAll(t1, t2, t3, t4, t5, t6);

        return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result);
    }

    /// <summary>
    /// Waits for all tasks to complete and returns their results as a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first task's result.</typeparam>
    /// <typeparam name="T2">The type of the second task's result.</typeparam>
    /// <typeparam name="T3">The type of the third task's result.</typeparam>
    /// <typeparam name="T4">The type of the fourth task's result.</typeparam>
    /// <typeparam name="T5">The type of the fifth task's result.</typeparam>
    /// <typeparam name="T6">The type of the sixth task's result.</typeparam>
    /// <typeparam name="T7">The type of the seventh task's result.</typeparam>
    /// <param name="t1">The first task.</param>
    /// <param name="t2">The second task.</param>
    /// <param name="t3">The third task.</param>
    /// <param name="t4">The fourth task.</param>
    /// <param name="t5">The fifth task.</param>
    /// <param name="t6">The sixth task.</param>
    /// <param name="t7">The seventh task.</param>
    /// <returns>A tuple containing the results of all tasks.</returns>
    public static async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(
        Task<T1> t1,
        Task<T2> t2,
        Task<T3> t3,
        Task<T4> t4,
        Task<T5> t5,
        Task<T6> t6,
        Task<T7> t7
    )
    {
        await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7);

        return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result, t7.Result);
    }

    /// <summary>
    /// Waits for all tasks to complete and returns their results as a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first task's result.</typeparam>
    /// <typeparam name="T2">The type of the second task's result.</typeparam>
    /// <typeparam name="T3">The type of the third task's result.</typeparam>
    /// <typeparam name="T4">The type of the fourth task's result.</typeparam>
    /// <typeparam name="T5">The type of the fifth task's result.</typeparam>
    /// <typeparam name="T6">The type of the sixth task's result.</typeparam>
    /// <typeparam name="T7">The type of the seventh task's result.</typeparam>
    /// <typeparam name="T8">The type of the eighth task's result.</typeparam>
    /// <param name="t1">The first task.</param>
    /// <param name="t2">The second task.</param>
    /// <param name="t3">The third task.</param>
    /// <param name="t4">The fourth task.</param>
    /// <param name="t5">The fifth task.</param>
    /// <param name="t6">The sixth task.</param>
    /// <param name="t7">The seventh task.</param>
    /// <param name="t8">The eighth task.</param>
    /// <returns>A tuple containing the results of all tasks.</returns>
    public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(
        Task<T1> t1,
        Task<T2> t2,
        Task<T3> t3,
        Task<T4> t4,
        Task<T5> t5,
        Task<T6> t6,
        Task<T7> t7,
        Task<T8> t8
    )
    {
        await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8);

        return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result, t7.Result, t8.Result);
    }
}
#pragma warning restore VSTHRD003
#pragma warning restore VSTHRD103
#pragma warning restore VSTHRD200
