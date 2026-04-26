# Core Patterns

Four patterns appear in almost every module and are worth understanding before reading individual packages.

## 1. Service Container

`IServiceContainer` wraps `Microsoft.Extensions.DependencyInjection.IServiceCollection`. Every module exposes a `ServiceContainerExtensions` class with fluent registration methods.

```csharp
var container = new ServiceContainer();            // base/Core/.../Container/ServiceContainer.cs:14
container.AddRuntime(assembly, tag: "app");
container.AddLogging(route => route.For(...).UseConsole());
container.AddSerializers()
         .WithJson()
         .WithMessagePack();
container.AddServicePack<MyFeaturePack>();

var provider = container.BuildServiceProvider();
```

Key guarantees:

- Every extension returns either the container or a scoped "builder" type so registration reads top-to-bottom.
- `ServiceContainer` holds an `IServiceCollection` internally — Microsoft DI stays the underlying substrate.
- Typed descriptors live in `base/Core/src/Annium.Core.DependencyInjection/Descriptors/` for features that need finer control than `IServiceCollection` offers.

## 2. Service Packs

A service pack is a reusable unit of composition. Instead of scattering `container.Add…` across startup, define a subclass of `ServicePackBase` (`base/Core/src/Annium.Core.DependencyInjection/Packs/ServicePackBase.cs:10`):

```csharp
public sealed class MyFeaturePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)          // 1. declare services
    {
        container.AddSerializers().WithJson();
        container.AddSingleton<IMyService, MyService>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)  // 2. post-config
    {
        var routing = provider.Resolve<IRouting>();
        container.AddSingleton(routing.BuildTable());
    }

    public override void Setup(IServiceProvider provider)                // 3. after provider built
    {
        provider.Resolve<IMyService>().Start();
    }
}
```

Phases (enforced by `IServiceProviderBuilder`):

| Phase | Purpose |
|-------|---------|
| `Configure` | Register services and options. The provider does not exist yet. |
| `Register` | Run once the provider is partially built — useful for services whose registration depends on resolving earlier ones. |
| `Setup` | Called after the final provider is built — ideal for kicking off hosted processes. |

`DynamicServicePack` composes registration logic from lambdas when creating a dedicated class would be overkill.

## 3. Result Pattern

`Annium.Data.Operations` defines three result families:

| Interface | File | Use When |
|-----------|------|----------|
| `IResult` / `IResult<T>` | `base/Data/src/Annium.Data.Operations/IResult.cs` | An operation can collect plain messages and optional data. |
| `IBooleanResult` / `IBooleanResult<T>` | `.../IBooleanResult.cs` | Success/failure is a single bit (with optional data). |
| `IStatusResult<TS>` / `IStatusResult<TS, TD>` | `.../IStatusResult.cs` | Outcome is one of a known finite status enum (with optional data). |

All three:

- Accumulate plain, labelled, and nested messages via `IResultBase`.
- Support deconstruction (`var (status, data) = result;`).
- Have JSON (`Annium.Data.Operations.Serialization.Json`) and MessagePack (`…MessagePack`) converters.
- Ship test assertions in `Annium.Data.Operations.Testing`.

Construct via the static `Result` factory:

```csharp
return Result.New().Fail("email", "invalid");            // IResult
return Result.Status(OperationStatus.NotFound);          // IStatusResult<OperationStatus>
return Result.Success(user).Has(Warning.Stale);          // IBooleanResult<User>
```

**Rule of thumb**: throw exceptions for programmer errors (null arg, impossible state), return results for domain outcomes (validation, not-found, conflict).

## 4. Testing

`TestBase` (`base/Annium/src/Annium.Testing/TestBase.cs:18`) replaces hand-wired test fixtures:

```csharp
public class OrdersTest(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void Create_ValidInput_Succeeds()
    {
        var orders = Provider.Resolve<IOrders>();
        var result = orders.Create(new("abc"));
        result.IsSuccess.IsTrue();
    }

    protected override void ConfigureContainer(IServiceContainer container)
    {
        container.AddMyFeaturePack();
    }
}
```

- `Provider` — `IKeyedServiceProvider` built lazily.
- `Logger` / `Logs` — in-memory log capture; inspect `Logs` to assert on telemetry.
- `OutputHelper` — xunit.v3 sink used both by xunit and the log router.
- Fluent assertions: `.Is()`, `.IsTrue()`, `.IsFalse()`, `.IsNotNull()`, `.Has(count)`, `.IsEmpty()`, `.IsEqual(expected)` (see `base/Annium/src/Annium.Testing/*Extensions.cs`).
- Exception testing: `Wrap.It(() => foo()).Throws<MyException>()` (`base/Annium/src/Annium.Testing/Wrap.cs:10`). Captures the expression text via `[CallerArgumentExpression]` so failure messages include `foo()`.

Testing conventions enforced across the repo:

- One test project per `src/` project, named `{Package}.Tests`.
- Method naming: `Method_Scenario_ExpectedResult` (e.g., `Parse_InvalidInput_Fails`).
- Tests inherit `TestBase` unless they truly need no DI/logging.
- xunit.v3 is the sole runner; `xunit.runner.visualstudio` wires it into `dotnet test`.

See the practical walkthrough in [Testing guide](../guides/testing.md).

## 5. Disconnect lifecycle

`Annium.Net.Sockets` and `Annium.Net.WebSockets` expose synchronous public `Disconnect()` /
`Dispose()` methods, but the underlying socket teardown is async. The clients reconcile this
with a fixed lifecycle invariant — added during T3, made observable from the public surface
during T8:

1. The caller invokes `socket.Disconnect()` (or `Dispose()`).
2. The status transitions to `Disconnected` *synchronously*, under the internal lock —
   `IsConnected` reports `false` from this moment.
3. `_socket.DisconnectAsync()` runs in a background `Task.Run` — the caller does not block.
4. After the underlying disconnect completes, the `OnDisconnected` event fires on the
   background continuation — handlers observe `IsConnected == false`.

This means: subscribers to `OnDisconnected` can rely on the underlying transport being torn
down by the time their handler runs, and `IsConnected` is a safe synchronous check at any
point. Async callers that need to wait for the event use the `WhenDisconnectedAsync`
extension; sync callers (e.g., resource-disposal handlers in `DisposableBox`) call
`Disconnect()` and continue without awaiting.

## 6. Testing: TestBase variants

The canonical fixture is `Annium.Testing.TestBase` (`base/Annium/src/Annium.Testing/TestBase.cs:18`).
Module-specific test projects subclass it where they need additional fixture scaffolding —
e.g., `Annium.Net.Sockets.Tests.TestBase` adds `RunServerBase(...)` for spinning up a real
loopback server. The hierarchy:

- **`Annium.Testing.TestBase`** — DI container with `AddRuntime` + time + logging
  pre-registered; exposes `Provider`, `Logger`, `Logs`, `OutputHelper`. Default for any unit
  test that needs DI.
- **`Annium.Net.Sockets.Tests.TestBase` / `Annium.Net.WebSockets.Tests.TestBase`** —
  inherits the canonical, adds `RunServerBase`/`RunServer` helpers and message-generation
  utilities for socket/websocket integration tests.
- **`Annium.Core.DependencyInjection.Tests.TestBase`** — inherits the canonical (T9
  consolidation); keeps a local `protected ServiceContainer Container` so DI-package tests
  can mutate the container directly without touching the inherited services.

When writing a new test class, prefer the canonical `Annium.Testing.TestBase` unless your
module already ships a subclass with relevant scaffolding.
