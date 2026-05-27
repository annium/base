using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>
/// Tests for <see cref="TestBase"/> IAsyncLifetime reshape: verifies that provider materialisation,
/// registration-window enforcement, and ergonomic overloads all behave as specified.
/// </summary>
public sealed class TestBaseLifecycleTests(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    // ---------------------------------------------------------------------------
    // Inner helpers
    // ---------------------------------------------------------------------------

    /// <summary>Minimal concrete subclass — no extra behaviour.</summary>
    private sealed class Subject(ITestOutputHelper outputHelper) : TestBase(outputHelper);

    /// <summary>
    /// Subclass that overrides <see cref="Testing.TestBase.InitializeAsync"/> and
    /// captures <see cref="Testing.TestBase.Provider"/> after chaining the base call.
    /// </summary>
    private sealed class OverridingSubject(ITestOutputHelper outputHelper) : TestBase(outputHelper)
    {
        /// <summary>Provider captured inside the overridden InitializeAsync, after base call.</summary>
        public IServiceProvider? CapturedProvider { get; private set; }

        /// <summary>
        /// Chains the base <see cref="Testing.TestBase.InitializeAsync"/> call and then captures
        /// the built <see cref="Testing.TestBase.Provider"/> into <see cref="CapturedProvider"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous initialisation.</returns>
        public override async ValueTask InitializeAsync()
        {
            await base.InitializeAsync();
            CapturedProvider = Provider;
        }
    }

    /// <summary>No-op service pack used to verify RegisterServicePack enforcement.</summary>
    private sealed class EmptyPack : ServicePackBase;

    /// <summary>Trivial marker service used to verify registrations wire through to the built provider.</summary>
    private sealed class Marker(string tag)
    {
        /// <summary>Gets the tag string supplied at construction time.</summary>
        public string Tag { get; } = tag;
    }

    // ---------------------------------------------------------------------------
    // Pre-init access throws
    // ---------------------------------------------------------------------------

    /// <summary>Accessing Provider before InitializeAsync throws with the documented diagnostic message.</summary>
    [Fact]
    public void Provider_AccessedBeforeInit_Throws()
    {
        var subject = new Subject(OutputHelper);

        var ex = Wrap.It(() => _ = subject.Provider).Throws<InvalidOperationException>();

        ex.Message.Contains("before InitializeAsync completed").IsTrue();
    }

    /// <summary>Accessing Logger before InitializeAsync throws with the documented diagnostic message.</summary>
    [Fact]
    public void Logger_AccessedBeforeInit_Throws()
    {
        var subject = new Subject(OutputHelper);

        var ex = Wrap.It(() => _ = subject.Logger).Throws<InvalidOperationException>();

        ex.Message.Contains("before InitializeAsync completed").IsTrue();
    }

    // ---------------------------------------------------------------------------
    // Registration window closes after InitializeAsync
    // ---------------------------------------------------------------------------

    /// <summary>Register(Action) called after InitializeAsync throws with the "frozen" message.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Register_CalledAfterInitStart_Throws()
    {
        var subject = new Subject(OutputHelper);
        await subject.InitializeAsync();

        var ex = Wrap.It(() => subject.Register(_ => { })).Throws<InvalidOperationException>();

        ex.Message.Contains("frozen").IsTrue();

        await subject.DisposeAsync();
    }

    /// <summary>Setup(Action) called after InitializeAsync throws with the "frozen" message.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Setup_CalledAfterInitStart_Throws()
    {
        var subject = new Subject(OutputHelper);
        await subject.InitializeAsync();

        var ex = Wrap.It(() => subject.Setup(_ => { })).Throws<InvalidOperationException>();

        ex.Message.Contains("frozen").IsTrue();

        await subject.DisposeAsync();
    }

    /// <summary>RegisterServicePack called after InitializeAsync throws with the "frozen" message.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RegisterServicePack_CalledAfterInitStart_Throws()
    {
        var subject = new Subject(OutputHelper);
        await subject.InitializeAsync();

        var ex = Wrap.It(() => subject.RegisterServicePack<EmptyPack>()).Throws<InvalidOperationException>();

        ex.Message.Contains("frozen").IsTrue();

        await subject.DisposeAsync();
    }

    /// <summary>Async Register(Func) overload called after InitializeAsync throws with the "frozen" message.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Register_AsyncOverload_CalledAfterInitStart_Throws()
    {
        var subject = new Subject(OutputHelper);
        await subject.InitializeAsync();

        var ex = Wrap.It(() => subject.Register((_, _) => Task.CompletedTask)).Throws<InvalidOperationException>();

        ex.Message.Contains("frozen").IsTrue();

        await subject.DisposeAsync();
    }

    /// <summary>Async Setup(Func) overload called after InitializeAsync throws with the "frozen" message.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Setup_AsyncOverload_CalledAfterInitStart_Throws()
    {
        var subject = new Subject(OutputHelper);
        await subject.InitializeAsync();

        var ex = Wrap.It(() => subject.Setup((_, _) => Task.CompletedTask)).Throws<InvalidOperationException>();

        ex.Message.Contains("frozen").IsTrue();

        await subject.DisposeAsync();
    }

    // ---------------------------------------------------------------------------
    // Post-init Provider and Logger are usable
    // ---------------------------------------------------------------------------

    /// <summary>After InitializeAsync completes, Provider and Logger are non-null and Get works.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task InitializeAsync_Completes_ProviderAndLoggerUsable()
    {
        var subject = new Subject(OutputHelper);
        await subject.InitializeAsync();

        Assert.NotNull(subject.Provider);
        Assert.NotNull(subject.Logger);
        Assert.NotNull(subject.Get<ILogger>());

        await subject.DisposeAsync();
    }

    // ---------------------------------------------------------------------------
    // Delegate overloads are invoked during init
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Async Register overload's delegate runs during InitializeAsync, receives a non-null container,
    /// receives a non-default CancellationToken, and services registered via the delegate are resolvable.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Register_AsyncOverload_RunsDelegateAndRegistersResolvableService()
    {
        var subject = new Subject(OutputHelper);
        IServiceContainer? capturedContainer = null;
        var ctReceived = false;

        subject.Register(
            (c, ct) =>
            {
                capturedContainer = c;
                ctReceived = ct.CanBeCanceled;
                c.Add(new Marker("from-async-register")).AsSelf().Singleton();
                return Task.CompletedTask;
            }
        );

        await subject.InitializeAsync();

        Assert.NotNull(capturedContainer);
        ctReceived.IsTrue();
        subject.Get<Marker>().Tag.Is("from-async-register");

        await subject.DisposeAsync();
    }

    /// <summary>
    /// Async Setup overload's delegate runs during InitializeAsync, receives a non-null provider,
    /// receives a non-default CancellationToken, and the captured provider matches the live Provider.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Setup_AsyncOverload_RunsDelegateWithProviderAndToken()
    {
        var subject = new Subject(OutputHelper);
        IServiceProvider? capturedProvider = null;
        var ctReceived = false;

        subject.Setup(
            (sp, ct) =>
            {
                capturedProvider = sp;
                ctReceived = ct.CanBeCanceled;
                return Task.CompletedTask;
            }
        );

        await subject.InitializeAsync();

        Assert.NotNull(capturedProvider);
        ctReceived.IsTrue();
        ReferenceEquals(capturedProvider, subject.Provider).IsTrue();

        await subject.DisposeAsync();
    }

    /// <summary>
    /// Sync Register overload's delegate runs during InitializeAsync and services registered via the
    /// delegate are resolvable — verifies the sync forwarder actually wires through to the builder.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Register_SyncOverload_RegistersResolvableService()
    {
        var subject = new Subject(OutputHelper);

        subject.Register(c => c.Add(new Marker("from-sync-register")).AsSelf().Singleton());

        await subject.InitializeAsync();

        subject.Get<Marker>().Tag.Is("from-sync-register");

        await subject.DisposeAsync();
    }

    /// <summary>
    /// Sync Setup overload's delegate runs during InitializeAsync and the captured provider equals
    /// the live Provider — verifies the sync forwarder actually wires through to the builder.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Setup_SyncOverload_RunsDelegateWithProvider()
    {
        var subject = new Subject(OutputHelper);
        IServiceProvider? capturedProvider = null;

        subject.Setup(sp => capturedProvider = sp);

        await subject.InitializeAsync();

        Assert.NotNull(capturedProvider);
        ReferenceEquals(capturedProvider, subject.Provider).IsTrue();

        await subject.DisposeAsync();
    }

    // ---------------------------------------------------------------------------
    // Subclass override sees built provider after base call
    // ---------------------------------------------------------------------------

    /// <summary>
    /// A subclass override of InitializeAsync that chains base can access Provider immediately
    /// after the base call returns.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SubclassInitializeAsync_AfterBaseAwait_SeesBuiltProvider()
    {
        var subject = new OverridingSubject(OutputHelper);
        await subject.InitializeAsync();

        Assert.NotNull(subject.CapturedProvider);
        ReferenceEquals(subject.CapturedProvider, subject.Provider).IsTrue();

        await subject.DisposeAsync();
    }

    // ---------------------------------------------------------------------------
    // DisposeAsync before init does not throw
    // ---------------------------------------------------------------------------

    /// <summary>DisposeAsync on a never-initialised subject completes without throwing.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_BeforeInit_DoesNotThrow()
    {
        var subject = new Subject(OutputHelper);

        // Should complete without exception even though _sp is null.
        await subject.DisposeAsync();
    }

    // ---------------------------------------------------------------------------
    // Logs accessor exposes the in-memory handler wired in SharedSetup
    // ---------------------------------------------------------------------------

    /// <summary>After init, emitted log messages surface through the <c>Logs</c> accessor.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Logs_AfterInitAndLog_ContainsEmittedMessage()
    {
        var subject = new Subject(OutputHelper);
        await subject.InitializeAsync();

        subject.Info("hello-from-test");

        // Logger uses an async dispatcher; allow the queued message to flush.
        for (var i = 0; i < 20 && subject.Logs.Count == 0; i++)
            await Task.Delay(25, TestContext.Current.CancellationToken);

        (subject.Logs.Count > 0).IsTrue();

        await subject.DisposeAsync();
    }
}
