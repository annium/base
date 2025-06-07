using System.Threading.Tasks;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Tests.Threading.Tasks;

/// <summary>
/// Contains unit tests for the TaskSet class.
/// </summary>
public class TaskSetTest
{
    /// <summary>
    /// Verifies that WhenAll works correctly with 2 tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_2()
    {
        // arrange
        var t1 = T(1);
        var t2 = T(true);

        // act & assert
        var (v1, v2) = await TaskSet.WhenAll(t1, t2);

        v1.Is(1);
        v2.Is(true);
    }

    /// <summary>
    /// Verifies that WhenAll works correctly with 3 tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_3()
    {
        // arrange
        var t1 = T(1);
        var t2 = T(true);
        var t3 = T(2m);

        // act & assert
        var (v1, v2, v3) = await TaskSet.WhenAll(t1, t2, t3);

        v1.Is(1);
        v2.Is(true);
        v3.Is(2m);
    }

    /// <summary>
    /// Verifies that WhenAll works correctly with 4 tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_4()
    {
        // arrange
        var t1 = T(1);
        var t2 = T(true);
        var t3 = T(2m);
        var t4 = T('x');

        // act & assert
        var (v1, v2, v3, v4) = await TaskSet.WhenAll(t1, t2, t3, t4);

        v1.Is(1);
        v2.Is(true);
        v3.Is(2m);
        v4.Is('x');
    }

    /// <summary>
    /// Verifies that WhenAll works correctly with 5 tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_5()
    {
        // arrange
        var t1 = T(1);
        var t2 = T(true);
        var t3 = T(2m);
        var t4 = T('x');
        var t5 = T("a");

        // act & assert
        var (v1, v2, v3, v4, v5) = await TaskSet.WhenAll(t1, t2, t3, t4, t5);

        v1.Is(1);
        v2.Is(true);
        v3.Is(2m);
        v4.Is('x');
        v5.Is("a");
    }

    /// <summary>
    /// Verifies that WhenAll works correctly with 6 tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_6()
    {
        // arrange
        var t1 = T(1);
        var t2 = T(true);
        var t3 = T(2m);
        var t4 = T('x');
        var t5 = T("a");
        var t6 = T(6f);

        // act & assert
        var (v1, v2, v3, v4, v5, v6) = await TaskSet.WhenAll(t1, t2, t3, t4, t5, t6);

        v1.Is(1);
        v2.Is(true);
        v3.Is(2m);
        v4.Is('x');
        v5.Is("a");
        v6.Is(6f);
    }

    /// <summary>
    /// Verifies that WhenAll works correctly with 7 tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_7()
    {
        // arrange
        var t1 = T(1);
        var t2 = T(true);
        var t3 = T(2m);
        var t4 = T('x');
        var t5 = T("a");
        var t6 = T(6f);
        var t7 = T(4u);

        // act & assert
        var (v1, v2, v3, v4, v5, v6, v7) = await TaskSet.WhenAll(t1, t2, t3, t4, t5, t6, t7);

        v1.Is(1);
        v2.Is(true);
        v3.Is(2m);
        v4.Is('x');
        v5.Is("a");
        v6.Is(6f);
        v7.Is(4u);
    }

    /// <summary>
    /// Verifies that WhenAll works correctly with 8 tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhenAll_8()
    {
        // arrange
        var t1 = T(1);
        var t2 = T(true);
        var t3 = T(2m);
        var t4 = T('x');
        var t5 = T("a");
        var t6 = T(6f);
        var t7 = T(4u);
        var t8 = T(8d);

        // act & assert
        var (v1, v2, v3, v4, v5, v6, v7, v8) = await TaskSet.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8);

        v1.Is(1);
        v2.Is(true);
        v3.Is(2m);
        v4.Is('x');
        v5.Is("a");
        v6.Is(6f);
        v7.Is(4u);
        v8.Is(8d);
    }

    /// <summary>
    /// Creates a completed task with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to wrap in a task.</param>
    /// <returns>A completed task containing the specified value.</returns>
    private Task<T> T<T>(T value) => Task.FromResult(value);
}
