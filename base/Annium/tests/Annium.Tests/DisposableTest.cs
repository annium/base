using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Tests;

public class DisposableTest : TestBase
{
    public DisposableTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

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