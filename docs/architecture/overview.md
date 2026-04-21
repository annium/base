# Architecture Overview

`Annium.Base` is the bottom of the Annium dependency stack. Every other Annium repo (`backend`, `frontend`, `id`, `finance`, `tools`, `xs`, `crypted`, …) depends on a subset of these packages. The sub-project is therefore structured for **stability, composability, and minimal coupling** rather than for any single application.

## Design Principles

1. **Modular packages, one solution.** Every leaf directory under `base/**/src/` is a standalone NuGet package. `Annium.Base.sln` aggregates them so a developer builds and tests everything with a single command, but consumers only pull the packages they need.
2. **Abstractions over frameworks.** `ServiceContainer` wraps `IServiceCollection`; `IResult`/`IStatusResult` wrap operation outcomes; `Annium.Logging.Shared` abstracts logging backends. Swapping the underlying framework (MS DI, JSON, Serilog…) does not leak into consumers.
3. **Results, not exceptions, for business flow.** Exceptions signal bugs; operation outcomes use the result interfaces in `Annium.Data.Operations`. See [Patterns](patterns.md).
4. **Service Packs encapsulate composition.** Rather than scattering registration across a project, features expose a `ServicePackBase` subclass with `Configure`/`Register`/`Setup` phases.
5. **Analyzer-enforced conventions.** `Annium.Analyzers` (shipped inside the `Annium` package) enforces exception naming and related rules. `WarningsAsErrors` is on; nullable reference types are on; XML docs on public APIs are mandatory.
6. **Central package version management.** `Directory.Packages.props` pins every third-party dependency; no project declares a `Version=` on `PackageReference`.

## System Shape

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ Consumers (other Annium repos: backend, frontend, id, finance, tools, xs…)   │
└────────────────────────────────────────┬─────────────────────────────────────┘
                                         │
                ┌────────────────────────┴──────────────────────┐
                │            Integrations (adapters)            │
                │    Graylog · NodaTime · Seq                   │
                └────────────────────────┬──────────────────────┘
                                         │
┌──────────────────┬──────────────────┬──┴───────────────┬──────────────────┐
│ Architecture     │ Identity         │ Net              │ Logging          │
│ Base · CQRS      │ Tokens · Jwt     │ Http · Sockets   │ Console · File   │
│ Http · Mediator  │                  │ Mail · WebSockets│ Microsoft · Seq  │
│ ViewModel        │                  │ Servers · Types  │ InMemory · Xunit │
├──────────────────┼──────────────────┴──────────────────┼──────────────────┤
│ Execution        │ Extensions                          │ Serialization    │
│ Background · Flow│ Arguments · CommandLine · Composition│ Abstractions    │
│                  │ Jobs · Pooling · Reactive · Shell   │ Json · Yaml      │
│                  │ Validation · Workers                │ MessagePack      │
├──────────────────┼─────────────────────────────────────┤ BinaryString     │
│ Configuration    │ Data                                ├──────────────────┤
│ Abstractions     │ Models · Operations · Tables        │ Localization     │
│ CommandLine      │ Operations.Serialization.*          │ Abstractions     │
│ Json · Yaml      │ Operations.Testing                  │ InMemory · Yaml  │
├──────────────────┴─────────────────────────────────────┴──────────────────┤
│ Core                                                                      │
│ DependencyInjection · Entrypoint · Mapper · Mediator · Runtime · Loader   │
├───────────────────────────────────────────────────────────────────────────┤
│ Annium (root package) · Annium.Analyzers · Annium.Testing                 │
└───────────────────────────────────────────────────────────────────────────┘
```

- **Annium** is the zero-dependency root: extension methods, disposables, time/id providers, collections, logging interfaces.
- **Annium.Testing** builds on `Annium` + `Annium.Logging.InMemory`/`Xunit` to give tests a DI host with captured logs.
- **Core** layers DI, mediator, mapper, and runtime assembly loading on top of `Annium`.
- **Data** supplies the result types, entity interfaces, and reactive table abstractions used across the stack.
- Higher tiers (**Architecture**, **Net**, **Identity**, **Serialization**, **Logging sinks**, **Extensions**) consume `Core`/`Data` and expose `ServiceContainerExtensions` for registration.
- **Integrations** sit outside `base/` to isolate third-party coupling (Graylog HTTP/GELF, NodaTime JSON, Seq sink).

## Repository Layout

```
base/                                # Sub-project root (git repo)
├── Annium.Base.sln                  # Single solution for every project
├── Directory.Build.props            # net10.0, Nullable, WarningsAsErrors, SourceLink
├── Directory.Packages.props         # Central package version management
├── global.json                      # SDK 10.0.0, rollForward=latestMinor
├── justfile                         # All recipes
├── docfx.json / toc.yml / index.md  # DocFX
├── base/
│   ├── Annium/                      # root package + Analyzers + Testing
│   ├── Architecture/                # Base · CQRS · Http · Mediator · ViewModel
│   ├── Configuration/               # Abstractions · CommandLine · Json · Yaml
│   ├── Core/                        # DependencyInjection · Entrypoint · Mapper · Mediator · Runtime · Runtime.Loader
│   ├── Data/                        # Models · Operations (+Json/MessagePack/Testing) · Tables
│   ├── Execution/                   # Background · Flow
│   ├── Extensions/                  # Arguments · CommandLine · Composition · Jobs · Pooling · Reactive · Shell · Validation · Workers
│   ├── Identity/                    # Tokens · Tokens.Jwt
│   ├── Localization/                # Abstractions · InMemory · Yaml
│   ├── Logging/                     # Console · File · InMemory · Microsoft · Shared · Xunit
│   ├── Net/                         # Base · Http · Mail · Servers.Sockets · Servers.Web · Sockets · Types (+Json) · WebSockets
│   └── Serialization/               # Abstractions · BinaryString · Json · MessagePack · Yaml
└── integrations/
    ├── Graylog/                     # Annium.Graylog.Logging
    ├── NodaTime/                    # Annium.NodaTime.Extensions · Serialization.Json
    └── Seq/                         # Annium.Seq.Logging
```

Every package follows the same shape:

```
base/<Group>/
├── Directory.Build.props            # optional group-level overrides
├── src/
│   └── <Package>/
│       ├── <Package>.csproj
│       └── *.cs
└── tests/
    └── <Package>.Tests/
        ├── <Package>.Tests.csproj
        └── *.cs
```

## Build Graph

`Annium.Base.sln` is the authoritative graph. A practical mental model of layer order:

1. `Annium.Analyzers` and `Annium` (root utilities)
2. `Annium.Testing` (tests across the whole tree use this)
3. `Annium.Core.*` — DI, Runtime, Mediator, Mapper
4. `Annium.Data.*`, `Annium.Logging.*`, `Annium.Serialization.*`
5. Vertical slices: `Annium.Architecture.*`, `Annium.Net.*`, `Annium.Identity.*`, `Annium.Extensions.*`, `Annium.Execution.*`, `Annium.Localization.*`, `Annium.Configuration.*`
6. `integrations/*` — adapters for external systems

Inside `base/Core/src/Annium.Core.DependencyInjection`:
- `Container/` — `IServiceContainer`, `ServiceContainer` (wraps `IServiceCollection`)
- `Builders/` — `IServiceProviderBuilder` used by `ServicePackBase`
- `Descriptors/` — typed service descriptors used by the fluent API
- `Packs/` — `ServicePackBase`, `DynamicServicePack`
- `Plugins/` — extension points for plugin-based DI

## Deployment Boundaries

- Packages are published to **nuget.org** (the umbrella repo configures API keys via `just copy-keys`).
- The `version` file holds the package version; `xx versioning get-version` turns it into the concrete `PackageVersion` supplied to `dotnet build`/`dotnet pack`.
- SourceLink is enabled (`Microsoft.SourceLink.GitHub`) so consumers can step into the source of published `.snupkg` symbol packages.
