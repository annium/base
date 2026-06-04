using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Entrypoint.Tests;

/// <summary>
/// Verifies that <see cref="Entrypoint"/> / <see cref="Entry"/> correctly unsubscribe from
/// static OS events (<c>Console.CancelKeyPress</c>, <c>AssemblyLoadContext.Default.Unloading</c>)
/// when the entry is disposed. Uses reflection on runtime-internal backing fields — brittle
/// across .NET versions; if internals move, the count assertions silently degrade to no-op.
/// </summary>
public class EntrypointDisposeTests
{
    /// <summary>
    /// Verifies that disposing three sequentially created entrypoints returns the
    /// <c>Console.CancelKeyPress</c> and <c>AssemblyLoadContext.Unloading</c> handler counts
    /// back to the baseline values measured before any entrypoint was created.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_ThreeEntrypoints_HandlerCountReturnsToBaseline()
    {
        // arrange — snapshot baseline (other tests may have handlers registered)
        var consoleBase = CountConsoleCancelKeyPressHandlers();
        var unloadBase = CountUnloadingHandlers();

        // act — create 3 entrypoints (each requires a ServicePack that registers ILogger)
        var entries = new List<Entry>();
        for (var i = 0; i < 3; i++)
            entries.Add(await new Entrypoint().UseServicePack<LoggingPack>().SetupAsync());

        // assert — each Setup should add 1 handler to each static event (if introspection works)
        var consoleAfterSetup = CountConsoleCancelKeyPressHandlers();
        var unloadAfterSetup = CountUnloadingHandlers();

        // Assert unconditionally: if introspection failed (count == -1) these fail loudly rather
        // than silently no-op'ing, since consoleBase/unloadBase would also be -1 and the +3 mismatch surfaces.
        consoleAfterSetup.Is(consoleBase + 3);
        unloadAfterSetup.Is(unloadBase + 3);

        // act — dispose all
        foreach (var e in entries)
            await e.DisposeAsync();

        // assert — handler counts back to baseline
        var consoleAfterDispose = CountConsoleCancelKeyPressHandlers();
        var unloadAfterDispose = CountUnloadingHandlers();

        consoleAfterDispose.Is(consoleBase);
        unloadAfterDispose.Is(unloadBase);
    }

    /// <summary>
    /// Attempts to read the invocation list length for <c>Console.CancelKeyPress</c>.
    /// Returns -1 when the runtime-internal backing field cannot be located.
    /// </summary>
    /// <returns>The number of registered handlers, or -1 if introspection is unavailable.</returns>
    private static int CountConsoleCancelKeyPressHandlers()
    {
        var field =
            typeof(Console).GetField("s_cancelCallbacks", BindingFlags.Static | BindingFlags.NonPublic)
            ?? typeof(Console).GetField("_cancelCallbacks", BindingFlags.Static | BindingFlags.NonPublic);
        if (field is null)
            return -1;
        var handler = (Delegate?)field.GetValue(null);
        return handler?.GetInvocationList().Length ?? 0;
    }

    /// <summary>
    /// Attempts to read the invocation list length for <c>AssemblyLoadContext.Default.Unloading</c>.
    /// Returns -1 when the runtime-internal backing field cannot be located.
    /// </summary>
    /// <returns>The number of registered handlers, or -1 if introspection is unavailable.</returns>
    private static int CountUnloadingHandlers()
    {
        var alc = AssemblyLoadContext.Default;
        // The Unloading event is implemented with explicit add/remove over a private backing
        // field named "_unloading" (not "Unloading"); fall back to "Unloading" for older runtimes.
        var field =
            typeof(AssemblyLoadContext).GetField("_unloading", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? typeof(AssemblyLoadContext).GetField("Unloading", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
            return -1;
        var handler = (Delegate?)field.GetValue(alc);
        return handler?.GetInvocationList().Length ?? 0;
    }

    /// <summary>
    /// Verifies that calling <see cref="Entrypoint.SetupAsync"/> a second time on the same
    /// <see cref="Entrypoint"/> instance (after a successful first call) throws
    /// <see cref="InvalidOperationException"/> due to the <c>_isAlreadyBuilt</c> guard.
    /// The first <see cref="Entry"/> is disposed before the assertion so that OS-event handlers
    /// registered by the first call are removed and do not leak into subsequent tests.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SetupAsync_CalledTwice_ThrowsInvalidOperationException()
    {
        // arrange — first call succeeds; dispose the entry immediately to clean up OS handlers
        var ep = new Entrypoint().UseServicePack<LoggingPack>();
        var entry = await ep.SetupAsync();
        await entry.DisposeAsync();

        // act + assert — second call on the same Entrypoint instance must throw
        await Wrap.It(async () => await ep.SetupAsync()).ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that when <see cref="Entrypoint.SetupAsync"/> fails during the build phase
    /// (i.e. a service pack's <c>ConfigureAsync</c> throws), the OS-event handlers that were
    /// wired before the build attempt are unsubscribed and the static handler counts return to
    /// the baseline measured before <c>SetupAsync</c> was called.
    /// Also verifies that a fresh <see cref="Entrypoint"/> with a working pack can still succeed
    /// after the failure, proving that the failure left no global poison state.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SetupAsync_BuildFails_UnsubscribesHandlersAndAllowsFreshEntrypoint()
    {
        // arrange — snapshot baseline before the failing entrypoint is created
        var consoleBase = CountConsoleCancelKeyPressHandlers();
        var unloadBase = CountUnloadingHandlers();

        // act — attempt to build; the ThrowingPack.ConfigureAsync throws, so SetupAsync throws
        var failingEp = new Entrypoint().UseServicePack<ThrowingPack>();
        Exception? caught = null;
        try
        {
            await failingEp.SetupAsync();
        }
        catch (Exception ex)
        {
            caught = ex;
        }

        // assert — an exception must have been thrown
        caught.IsNotNull();

        // assert — handler counts returned to baseline (cleanup ran on the failure path)
        CountConsoleCancelKeyPressHandlers().Is(consoleBase);
        CountUnloadingHandlers().Is(unloadBase);

        // assert — a fresh entrypoint with a working pack still succeeds (no global poison)
        var freshEntry = await new Entrypoint().UseServicePack<LoggingPack>().SetupAsync();
        await freshEntry.DisposeAsync();
    }

    /// <summary>
    /// Minimal service pack that wires <c>ILogger</c> via <c>AddLogging</c>.
    /// Entry resolves <c>ILogger</c> in its initializer, so this must be present.
    /// </summary>
    private sealed class LoggingPack : ServicePackBase
    {
        /// <summary>Registers time and logging services into the container.</summary>
        /// <param name="container">The service container to configure.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A completed task.</returns>
        public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
        {
            container.AddTime().WithRealTime().SetDefault();
            container.AddLogging();
            return Task.CompletedTask;
        }

        /// <summary>Wires in-memory logging so the entrypoint's ILogger resolution succeeds.</summary>
        /// <param name="container">The service container (unused; logging routing is via provider).</param>
        /// <param name="provider">The built service provider used to configure logging routes.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A completed task.</returns>
        public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
        {
            provider.UseLogging(route => route.UseInMemory());
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Service pack whose <c>ConfigureAsync</c> always throws an <see cref="InvalidOperationException"/>,
    /// simulating a build-phase failure so that the failure-path cleanup logic in
    /// <see cref="Entrypoint.SetupAsync"/> can be exercised.
    /// </summary>
    private sealed class ThrowingPack : ServicePackBase
    {
        /// <summary>
        /// Always throws <see cref="InvalidOperationException"/> to simulate a build failure.</summary>
        /// <param name="container">The service container (not used).</param>
        /// <param name="ct">Cancellation token (not used).</param>
        /// <returns>Never returns normally.</returns>
        public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct) =>
            throw new InvalidOperationException("Simulated build failure in ThrowingPack");
    }
}
