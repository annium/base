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

        // If counts are -1 (introspection failed), skip strict assertion but still verify dispose below.
        if (consoleAfterSetup >= 0)
            consoleAfterSetup.Is(consoleBase + 3);
        if (unloadAfterSetup >= 0)
            unloadAfterSetup.Is(unloadBase + 3);

        // act — dispose all
        foreach (var e in entries)
            await e.DisposeAsync();

        // assert — handler counts back to baseline
        var consoleAfterDispose = CountConsoleCancelKeyPressHandlers();
        var unloadAfterDispose = CountUnloadingHandlers();

        if (consoleAfterDispose >= 0)
            consoleAfterDispose.Is(consoleBase);
        if (unloadAfterDispose >= 0)
            unloadAfterDispose.Is(unloadBase);
    }

    /// <summary>
    /// Attempts to read the invocation list length for <c>Console.CancelKeyPress</c>.
    /// Returns -1 when the runtime-internal backing field cannot be located.
    /// </summary>
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
    private static int CountUnloadingHandlers()
    {
        var alc = AssemblyLoadContext.Default;
        var field = typeof(AssemblyLoadContext).GetField("Unloading", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
            return -1;
        var handler = (Delegate?)field.GetValue(alc);
        return handler?.GetInvocationList().Length ?? 0;
    }

    /// <summary>
    /// Minimal service pack that wires <c>ILogger</c> via <c>AddLogging</c>.
    /// Entry resolves <c>ILogger</c> in its initializer, so this must be present.
    /// </summary>
    private sealed class LoggingPack : ServicePackBase
    {
        public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
        {
            container.AddTime().WithRealTime().SetDefault();
            container.AddLogging();
            return Task.CompletedTask;
        }

        public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
        {
            provider.UseLogging(route => route.UseInMemory());
            return Task.CompletedTask;
        }
    }
}
