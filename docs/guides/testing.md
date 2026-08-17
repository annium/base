# Testing

`Annium.Testing` is a thin layer over xunit.v3 that adds a DI host, captured logs, and fluent assertions.

## Project layout

Every `src/<Package>` has a sibling `tests/<Package>.Tests`. The test csproj references:

- `xunit.v3` + `xunit.v3.extensibility.core`
- `Annium.Testing` (this project)
- The package under test

Versions are pinned in `Directory.Packages.props`.

Test projects are `<OutputType>Exe</OutputType>` — xunit.v3 runs on
[Microsoft.Testing.Platform](https://aka.ms/dotnet-test) (MTP), not VSTest. The repo opts into the MTP
`dotnet test` experience via `global.json` (`"test": { "runner": "Microsoft.Testing.Platform" }`).

## TestBase

`base/Annium/src/Annium.Testing/TestBase.cs:18` is an abstract class that gives every test a fully wired host:

```csharp
public sealed class AddTest(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void Add_TwoPositives_Returns_Sum()
    {
        var calc = Provider.Resolve<ICalculator>();
        calc.Add(2, 3).Is(5);
    }

    protected override void ConfigureContainer(IServiceContainer container)
    {
        container.AddSingleton<ICalculator, Calculator>();
    }
}
```

Exposed surface (see `TestBase.cs` for full doc):

| Member | Purpose |
|--------|---------|
| `Provider` | `IKeyedServiceProvider`, lazily built from the configured container. |
| `Logger` | `ILogger` whose messages are captured. |
| `Logs` | `IReadOnlyList<LogMessage<DefaultLogContext>>` — assert on telemetry. |
| `OutputHelper` | xunit.v3 `ITestOutputHelper`. |

Override `ConfigureContainer(IServiceContainer)` to add test-scoped registrations. The base class pre-registers runtime, logging (routed through `Annium.Logging.Xunit` to `OutputHelper` and mirrored into `Annium.Logging.InMemory`), and DI plumbing.

## TestBase variants

The canonical `Annium.Testing.TestBase` covers most unit tests. A few modules ship subclasses
that add fixture-heavy scaffolding for integration tests:

| Class | Adds | When to use |
|-------|------|-------------|
| `Annium.Testing.TestBase` | DI + runtime + time + logging | Default for any test that needs DI |
| `Annium.Net.Sockets.Tests.TestBase` | `RunServerBase(handle)` to spin up a loopback server, `GenerateMessage(s)` utilities | Socket integration tests against a real server |
| `Annium.Net.WebSockets.Tests.TestBase` | Same shape as the sockets variant for WebSockets | WebSocket integration tests |
| `Annium.Core.DependencyInjection.Tests.TestBase` | Local `Container` field for direct mutation; `Build()` and `Get<T>()` helpers | Tests that mutate a container without touching the inherited services |

When writing a new test class, prefer the canonical base unless your module already ships a
subclass with relevant scaffolding. New module-specific subclasses should inherit
`Annium.Testing.TestBase` (not `xunit.v3` directly) so they pick up the canonical DI/logging
plumbing for free.

## Fluent assertions

Located in `base/Annium/src/Annium.Testing/*Extensions.cs`.

| Extension | Example |
|-----------|---------|
| `.Is(expected)` | `x.Is(42)` — value equality |
| `.IsEqual(expected)` | `obj.IsEqual(other)` — deep/shallow via `ShallowEqualityExtensions` |
| `.IsTrue()` / `.IsFalse()` | `ready.IsTrue()` |
| `.IsNotNull()` / `.IsNull()` | `instance.IsNotNull()` |
| `.IsDefault()` / `.IsNotDefault()` | `result.IsNotDefault()` |
| `.Has(count)` / `.IsEmpty()` | `list.Has(3)`, `seq.IsEmpty()` |
| `.IsGreaterThan(x)` / `.IsLessThan(x)` | numeric comparisons |

All assertions throw `AssertionFailedException` on failure. They work on any value because they are plain extension methods — no wrapping like `Assert.That(...)` or `.Should()`.

## Exception testing with Wrap

`Wrap.It(...)` (`base/Annium/src/Annium.Testing/Wrap.cs:10`) wraps a delegate plus its expression text:

```csharp
Wrap.It(() => parser.Parse("xyz")).Throws<FormatException>();

// async
await Wrap.It(async () => await client.FetchAsync()).ThrowsAsync<HttpRequestException>();
```

`[CallerArgumentExpression]` captures `parser.Parse("xyz")` into the failure message, so a mismatched exception prints something like:

```
Expected 'parser.Parse("xyz")' to throw FormatException, but InvalidDataException was thrown.
```

## Test naming

The convention `Method_Scenario_ExpectedResult` is used across every test project. Examples from the codebase:

- `Parse_InvalidInput_Fails`
- `Add_TwoPositives_Returns_Sum`
- `Handle_MissingHeader_ReturnsBadRequest`

Matching this pattern makes filter-based running predictable:

```bash
dotnet test --filter "FullyQualifiedName~Parse_InvalidInput"
```

## Log-based assertions

`TestBase.Logs` exposes the in-memory log handler:

```csharp
calc.Add(2, 3);
Logs.Count.IsGreaterThan(0);
Logs[0].Message.Is("add called");
```

This is the preferred way to test side-effects that surface only through logs.

## Per-test fixtures

Where xunit.v3 fixtures are needed (e.g., shared TCP server), place them in the test project next to the tests that use them. Several `base/Net/tests/*/TestBase.cs` files demonstrate the pattern of subclassing `TestBase` with extra fixture lifecycle logic.

## Running

```bash
just test                                     # every project, Release, TRX report
dotnet test --project base/Core/tests/Annium.Core.Mediator.Tests/
dotnet test --filter "ClassName=SomeTest"
dotnet test --filter "Category=Integration"   # if you add xunit categories
```

CI uses `just ci-merge-request-full` which calls `just test` after a clean build.
