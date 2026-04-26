# Annium.Base

Modular .NET 10 framework providing the foundational libraries that every other Annium sub-project builds on: core utilities, DI, CQRS/mediator, configuration, data/result types, networking, serialization, identity, logging, and testing.

## Important: Documentation Requirements

All code changes MUST be reflected in documentation. When making changes:

1. Update the relevant file in `docs/` for architecture or pattern changes
2. Keep this `CLAUDE.md` in sync with changes to `justfile`, `Directory.Build.props`, `Directory.Packages.props`, or the module layout under `base/`
3. Run `/document-repository` to refresh docs after significant changes
4. Public APIs require XML doc comments — `make docs-lint` / `just docs-lint` enforces this

## Quick Reference

| Command | Description |
|---------|-------------|
| `just setup` | Restore dotnet tools (CSharpier, xs.cli, doclint, docfx, versioning) |
| `just format` | Format with CSharpier + `xs format` |
| `just build` | Build `Annium.Base.sln` in Release |
| `just test` | Run all tests (xunit.v3, TRX logger) |
| `just clean` | `xs clean` and remove stray `*.nupkg` |
| `just docs-build` | Build DocFX site into `_site/` |
| `just` | List every recipe |

Single test: `dotnet test base/<Module>/tests/<Module>.Tests/<Module>.Tests.csproj --filter "FullyQualifiedName~Name"`.

## Project Structure

```
base/                                  # Sub-project root
├── Annium.Base.sln                    # One solution for every project
├── Directory.Build.props              # TargetFramework=net10.0, WarningsAsErrors, SourceLink
├── Directory.Packages.props           # Central package version management
├── global.json                        # SDK 10.0.0, latestMinor, allowPrerelease
├── .csharpierrc                       # 120 cols, 4 spaces, DEBUG/CODE_STYLE symbols
├── .editorconfig                      # Style rules + analyzer severities
├── justfile                           # All recipes (replaces former makefile)
├── nuget.config                       # Feed configuration
├── version                            # Base version string consumed by `xx versioning`
├── docfx.json / toc.yml / index.md    # DocFX metadata and landing page
├── base/                              # All first-party modules
│   ├── Annium/                        # Core utilities, Annium.Analyzers, Annium.Testing
│   ├── Architecture/                  # Base, CQRS, Http, Mediator, ViewModel
│   ├── Configuration/                 # Abstractions, CommandLine, Json, Yaml
│   ├── Core/                          # DependencyInjection, Entrypoint, Mapper, Mediator, Runtime, Runtime.Loader
│   ├── Data/                          # Models, Operations (+Json/MessagePack/Testing), Tables
│   ├── Execution/                     # Background, Flow
│   ├── Extensions/                    # Arguments, CommandLine, Composition, Jobs, Pooling, Reactive, Shell, Validation, Workers
│   ├── Identity/                      # Tokens, Tokens.Jwt
│   ├── Localization/                  # Abstractions, InMemory, Yaml
│   ├── Logging/                       # Console, File, InMemory, Microsoft, Shared, Xunit
│   ├── Net/                           # Base, Http, Mail, Servers.Sockets, Servers.Web, Sockets, Types (+Json), WebSockets
│   └── Serialization/                 # Abstractions, BinaryString, Json, MessagePack, Yaml
├── integrations/                      # Third-party adapters
│   ├── Graylog/                       # Annium.Graylog.Logging
│   ├── NodaTime/                      # Annium.NodaTime.Extensions, Serialization.Json
│   └── Seq/                           # Annium.Seq.Logging
├── api/                               # DocFX-generated API YAML (gitignored)
└── _site/                             # DocFX-generated site (gitignored)
```

Every module follows the same shape: `base/<Group>/src/<Package>/` + `base/<Group>/tests/<Package>.Tests/`.

## Key Concepts

- **Modular package graph** — each leaf directory under `src/` is an independently packable NuGet project; `Annium.{Group}.{Module}` naming convention.
- **`IServiceContainer` abstraction** — wraps `IServiceCollection` with fluent extensions (`base/Core/src/Annium.Core.DependencyInjection/Container/ServiceContainer.cs:14`). Consumers register features through extension methods on this type.
- **Service Pack pattern** — `ServicePackBase` (`base/Core/src/Annium.Core.DependencyInjection/Packs/ServicePackBase.cs:10`) has three phases: `Configure` (wire services), `Register` (post-config registration with access to provider), `Setup` (post-build initialization).
- **Result pattern** — `IResult`, `IResult<T>` (`base/Data/src/Annium.Data.Operations/IResult.cs`), `IBooleanResult<T>` (`…/IBooleanResult.cs`), `IStatusResult<TS, TD>` (`…/IStatusResult.cs`). Business failures are returned as results, not thrown.
- **`TestBase`** — DI + logging host for tests, exposing `Provider`, `Logger`, `Logs`, `OutputHelper` (`base/Annium/src/Annium.Testing/TestBase.cs:18`). Each test class inherits this instead of wiring DI manually.
- **`Wrap.It(...)`** — delegate wrapper used with `.Throws<T>()` assertions (`base/Annium/src/Annium.Testing/Wrap.cs:10`). Captures the source expression via `[CallerArgumentExpression]` for richer diagnostics.
- **`Annium.Analyzers`** — ships with the `Annium` package, enforces exception-naming and related conventions at build time.

## Configuration

| Concern | File | Notes |
|---------|------|-------|
| Target framework / warnings | `Directory.Build.props` | `net10.0`, `Nullable=enable`, `WarningsAsErrors`, SourceLink enabled |
| Package versions | `Directory.Packages.props` | Central — `ManagePackageVersionsCentrally=true`; xunit.v3, MessagePack, NodaTime, etc. pinned here |
| SDK pin | `global.json` | `version=10.0.0`, `rollForward=latestMinor`, `allowPrerelease=true` |
| Formatter | `.csharpierrc` + `.editorconfig` | 120 col width, 4-space indent; CSharpier run via tool manifest |
| Analyzers | `Directory.Build.props` (`EnableNETAnalyzers`, `AnalysisMode=Default`) + `base/Annium/src/Annium.Analyzers` | Exception-naming, visibility rules; `AD0001` suppressed globally |
| Package version at build | `just build` reads `./version` via `xx versioning get-version` | Passed to MSBuild as `PackageVersion` |
| NuGet credentials | `.xs.credentials` (gitignored) | Provisioned by the umbrella `just copy-keys` in `/Users/alex/Projects/annium` |

## .NET Solution Structure

- **One solution** — `Annium.Base.sln` contains every `src/` and `tests/` project. Rebuild the solution graph after adding a project (`dotnet sln add …`).
- **Per-group `Directory.Build.props`** — each `base/<Group>/` folder can override metadata (e.g., package description) for its members.
- **Naming** — package/assembly name equals the project folder name (e.g., `Annium.Core.Mediator`).
- **Test projects** — always named `{Package}.Tests`, live next to the subject under `tests/`, reference xunit.v3 + `Annium.Testing`.

## Service Registration

Modules expose fluent registration via `ServiceContainerExtensions` (see any `base/**/ServiceContainerExtensions.cs`). Typical flow:

```csharp
var container = new ServiceContainer();
container.AddRuntime(...);          // from Annium.Core.Runtime
container.AddLogging(...);          // from Annium.Logging.Shared
container.AddSerializers()          // from Annium.Serialization.Abstractions
         .WithJson()
         .WithMessagePack();
// Service packs encapsulate multi-step registration
container.AddServicePack<MyFeaturePack>();
```

`ServicePackBase` has three virtual hooks (`Configure` → `Register` → `Setup`) called by `IServiceProviderBuilder` when the provider is built.

## Testing

- **Framework**: xunit.v3 + `xunit.runner.visualstudio`.
- **Base class**: inherit `Annium.Testing.TestBase` to get DI, in-memory logs, xunit log bridge, and `OutputHelper`.
- **Assertions**: fluent extensions on any value — `.Is(expected)`, `.IsTrue()`, `.IsFalse()`, `.IsNotNull()`, `.Has(count)`, `.IsEmpty()`, `.IsEqual(expected)` (see `base/Annium/src/Annium.Testing/*Extensions.cs`).
- **Exception testing**: `Wrap.It(() => SomeAction()).Throws<SomeException>()`.
- **Naming**: `Method_Scenario_ExpectedResult` (e.g., `Parse_InvalidInput_Fails`).
- **Run a subset**: `dotnet test --filter "FullyQualifiedName~TestBase"` or `--filter "ClassName.MethodName"`.
- **Traits**: `make test` / `just test` writes TRX logs named `test-results.trx` per project.

## Documentation

**Architecture**
- [Overview](docs/architecture/overview.md) — system design, module map, design principles
- [Modules](docs/architecture/modules.md) — per-module responsibilities and surface types
- [Patterns](docs/architecture/patterns.md) — Result, Service Packs, DI, Testing conventions

**Guides**
- [Development](docs/guides/development.md) — build / format / test / pack / publish via `just`
- [Testing](docs/guides/testing.md) — `TestBase`, `Wrap`, fluent assertions, xunit.v3 specifics
- [Documentation](docs/guides/documentation.md) — DocFX metadata/site flow and `doclint`

## Development Commands

### Build / test / package
| Command | Description |
|---------|-------------|
| `just setup` | `dotnet tool restore` — pulls the tool manifest (CSharpier, xs, doclint, docfx, versioning) |
| `just format` | CSharpier + `xs format -sc -ic` |
| `just format-full` | Also runs `dotnet format style` and `dotnet format analyzers` |
| `just ensure-no-changes` | CI guard: fails if the working tree is dirty |
| `just build` | Computes package version from `./version`, builds Release |
| `just test` | Runs xunit.v3 tests with TRX logger |
| `just clean` | `xs clean` + removes stray `*.nupkg` |
| `just update` | Reinstalls all dotnet tools and runs `xs update all` |
| `just pack` | Creates `.nupkg` + `.snupkg` with computed version |
| `just publish <apiKey>` | Pushes `*.nupkg` to nuget.org |

### Documentation
| Command | Description |
|---------|-------------|
| `just docs-lint` | `doclint lint` — enforces XML doc requirements on `**/*.cs` |
| `just docs-metadata` | `docfx metadata docfx.json` — regenerates `api/` YAML |
| `just docs-build` | Build static site into `_site/` |
| `just docs-serve` | Serve `_site/` locally |
| `just docs-watch` | Rebuild + serve with file watching |
| `just docs-clean` | Remove `_site/` and `api/` |

### Keys (test fixtures)
| Command | Description |
|---------|-------------|
| `just gen-rsa-keys` | Generate RSA key pair + self-signed cert + PFX |
| `just copy-rsa-keys` | Deploy RSA keys into `Annium.Identity.Tokens.*` and `Annium.Net.Sockets` test fixture folders |
| `just gen-ec-keys` | Generate EC (secp521r1) key pair + cert + PFX |
| `just copy-ec-keys` | Deploy EC keys into the same fixture folders |

### CI
| Command | Description |
|---------|-------------|
| `just ci-merge-request-short` | setup → format → ensure-no-changes → clean → build |
| `just ci-merge-request-full` | short pipeline + `test` |
| `just ci-release <apiKey> <repo> <ghToken>` | Full release: set package version, pack, publish, push tag |
| `just ci-set-package-version` | Sets repo version via `xx versioning set-version` |
| `just ci-push-tag <repo> <ghToken>` | Pushes `v<packageVersion>` tag to origin |

> `make` targets still work historically; the canonical entry point for this repo is now `just` (see `justfile`).
