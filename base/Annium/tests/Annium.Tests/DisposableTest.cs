using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for <see cref="Disposable"/> to verify disposable behavior.
/// </summary>
public class DisposableTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public DisposableTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that adding disposables to an async disposable box works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_Add_Works()
    {
        // arrange
        var box = Disposable.AsyncBox(Get<ILogger>());
        var calls = 0;

        // act
        box += Disposable.Create(() => ++calls);
        box += Disposable.Create(() => Task.FromResult(++calls));
        box += () => ++calls;
        box += () => Task.FromResult(++calls);
        await box.DisposeAsync();

        // assert
        calls.Is(4);
    }

    /// <summary>
    /// Verifies that removing disposables from an async disposable box works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_Remove_Works()
    {
        // arrange
        var box = Disposable.AsyncBox(Get<ILogger>());
        var calls = 0;

        // act
        var disposable = Disposable.Create(() => ++calls);
        var asyncDisposable = Disposable.Create(() => Task.FromResult(++calls));
        void Dispose() => ++calls;
        Task AsyncDispose() => Task.FromResult(++calls);
        box += disposable;
        box -= disposable;
        box += asyncDisposable;
        box -= asyncDisposable;
        box += Dispose;
        box -= Dispose;
        box += AsyncDispose;
        box -= AsyncDispose;
        await box.DisposeAsync();

        // assert
        calls.Is(0);
    }

    /// <summary>
    /// Verifies that resetting an async disposable box works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncDisposable_Reset_Works()
    {
        // arrange
        var box = Disposable.AsyncBox(Get<ILogger>());

        // act
        await box.DisposeAndResetAsync();

        // assert
        box.IsDisposed.IsFalse();
    }

    /// <summary>
    /// Verifies that adding disposables to a disposable box works correctly.
    /// </summary>
    [Fact]
    public void Disposable_Add_Works()
    {
        // arrange
        var box = Disposable.Box(Get<ILogger>());
        var calls = 0;

        // act
        box += Disposable.Create(() => ++calls);
        box += () => ++calls;
        box.Dispose();

        // assert
        calls.Is(2);
    }

    /// <summary>
    /// Verifies that removing disposables from a disposable box works correctly.
    /// </summary>
    [Fact]
    public void Disposable_Remove_Works()
    {
        // arrange
        var box = Disposable.Box(Get<ILogger>());
        var calls = 0;

        // act
        var disposable = Disposable.Create(() => ++calls);
        void Dispose() => ++calls;
        box += disposable;
        box -= disposable;
        box += Dispose;
        box -= Dispose;
        box.Dispose();

        // assert
        calls.Is(0);
    }

    /// <summary>
    /// Verifies that resetting a disposable box works correctly.
    /// </summary>
    [Fact]
    public void Disposable_Reset_Works()
    {
        // arrange
        var box = Disposable.Box(Get<ILogger>());

        // act
        box.DisposeAndReset();

        // assert
        box.IsDisposed.IsFalse();
    }
}
