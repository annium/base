# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

**Essential Commands:**
- `make setup` - Restore dotnet tools (CSharpier, xs.cli)
- `make format` - Format code using CSharpier and xs format
- `make build` - Build solution in Release configuration
- `make test` - Run all tests with TRX logging
- `make clean` - Clean solution and remove packages

**Single Test Execution:**
```bash
dotnet test path/to/TestProject.csproj --filter "TestMethodName"
dotnet test --filter "ClassName.TestMethodName"
```

**Package Management:**
- `make update` - Update all packages using xs tool
- `make pack` - Create NuGet packages
- Uses central package management via `Directory.Packages.props`

## Architecture Overview

This is a modular .NET 9.0 framework organized into logical domains:

**Core Framework Structure:**
- `base/Annium/` - Core utilities, extensions, testing framework
- `base/Core/` - DI, Mediator, Mapper, Runtime fundamentals  
- `base/Architecture/` - CQRS, HTTP, ViewModel patterns
- `base/Data/` - Models, Result patterns, Tables
- `base/Configuration/` - Multi-provider configuration system
- `base/Net/` - HTTP, Sockets, WebSockets, Mail
- `base/Serialization/` - JSON, MessagePack, YAML serializers
- `base/Extensions/` - Jobs, Validation, Workers, Pooling, etc.
- `integrations/` - Third-party integrations (Graylog, NodaTime, Seq)

## Key Patterns

**Testing Framework:**
- Custom `TestBase` class with DI and logging setup
- Fluent assertions: `.Is()`, `.IsTrue()`, `.Has()`, `.IsEmpty()`
- Exception testing: `Wrap.It().Throws<ExceptionType>()`
- Test naming: `MethodName_Scenario_ExpectedResult()`

**Result Pattern:**
All operations return structured results (`IResult<T>`, `IBooleanResult`, `IStatusResult`) instead of throwing exceptions for business logic failures.

**Service Registration:**
- Use `ServiceContainer` abstraction over Microsoft.Extensions.DI
- "Service pack" pattern for modular feature registration
- Extension methods for fluent configuration APIs

**Code Quality:**
- Warnings as errors with nullable reference types
- Custom analyzers for exception naming conventions
- XML documentation required for public APIs
- CSharpier formatting with .editorconfig rules

## Project Conventions

- Each module has `src/` and `tests/` directories
- Test projects named `{Module}.Tests`
- Shared build configuration via `Directory.Build.props` inheritance
- All projects target .NET 9.0 with latest C# language version
- Use `Annium.{Module}.{SubModule}` naming convention