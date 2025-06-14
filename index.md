---
_layout: landing
---

# Annium.Base Documentation

Annium.Base is a modular .NET 9.0 framework providing essential utilities, patterns, and abstractions for building robust applications.

## Core Framework

### Base Components
- **[Annium](api/base/Annium/Annium/Annium.yml)** - Core utilities, extensions, and fundamental types
- **[Annium.Analyzers](api/base/Annium/Annium.Analyzers/Annium.Analyzers.yml)** - Code analyzers and fixes for maintaining code quality
- **[Annium.Testing](api/base/Annium/Annium.Testing/Annium.Testing.yml)** - Testing framework with fluent assertions and utilities

## Architecture

Architectural patterns and abstractions for building well-structured applications:

- **[Annium.Architecture.Base](api/base/Architecture/Annium.Architecture.Base/Annium.Architecture.Base.yml)** - Base architectural components and operation status types
- **[Annium.Architecture.CQRS](api/base/Architecture/Annium.Architecture.CQRS/Annium.Architecture.CQRS.yml)** - Command Query Responsibility Segregation pattern implementation
- **[Annium.Architecture.Http](api/base/Architecture/Annium.Architecture.Http/Annium.Architecture.Http.yml)** - HTTP-based architectural patterns and mediator extensions
- **[Annium.Architecture.Mediator](api/base/Architecture/Annium.Architecture.Mediator/Annium.Architecture.Mediator.yml)** - Mediator pattern implementation for request/response handling
- **[Annium.Architecture.ViewModel](api/base/Architecture/Annium.Architecture.ViewModel/Annium.Architecture.ViewModel.yml)** - ViewModel pattern abstractions and request/response interfaces

## Configuration

Multi-provider configuration management system:

- **[Annium.Configuration.Abstractions](api/base/Configuration/Annium.Configuration.Abstractions/Annium.Configuration.Abstractions.yml)** - Core configuration abstractions and builder interfaces
- **[Annium.Configuration.CommandLine](api/base/Configuration/Annium.Configuration.CommandLine/Annium.Configuration.CommandLine.yml)** - Command-line argument configuration provider
- **[Annium.Configuration.Json](api/base/Configuration/Annium.Configuration.Json/Annium.Configuration.Json.yml)** - JSON file configuration provider
- **[Annium.Configuration.Yaml](api/base/Configuration/Annium.Configuration.Yaml/Annium.Configuration.Yaml.yml)** - YAML file configuration provider

## Core Services

Fundamental services for dependency injection, mediation, mapping, and runtime management:

- **[Annium.Core.DependencyInjection](api/base/Core/Annium.Core.DependencyInjection/Annium.Core.DependencyInjection.yml)** - Service container abstractions and dependency injection utilities
- **[Annium.Core.Entrypoint](api/base/Core/Annium.Core.Entrypoint/Annium.Core.Entrypoint.yml)** - Application entry point management and run pack utilities
- **[Annium.Core.Mapper](api/base/Core/Annium.Core.Mapper/Annium.Core.Mapper.yml)** - Object mapping framework with profile-based configuration
- **[Annium.Core.Mediator](api/base/Core/Annium.Core.Mediator/Annium.Core.Mediator.yml)** - Core mediator pattern implementation for request handling
- **[Annium.Core.Runtime](api/base/Core/Annium.Core.Runtime/Annium.Core.Runtime.yml)** - Runtime utilities and assembly management
- **[Annium.Core.Runtime.Loader](api/base/Core/Annium.Core.Runtime.Loader/Annium.Core.Runtime.Loader.yml)** - Dynamic assembly loading capabilities

## Data

Data models, operations, and table abstractions:

- **[Annium.Data.Models](api/base/Data/Annium.Data.Models/Annium.Data.Models.yml)** - Base data models, entity interfaces, and value range types
- **[Annium.Data.Operations](api/base/Data/Annium.Data.Operations/Annium.Data.Operations.yml)** - Result pattern implementation for data operations
- **[Annium.Data.Operations.Serialization.Json](api/base/Data/Annium.Data.Operations.Serialization.Json/Annium.Data.Operations.Serialization.Json.yml)** - JSON serialization support for operation results
- **[Annium.Data.Operations.Serialization.MessagePack](api/base/Data/Annium.Data.Operations.Serialization.MessagePack/Annium.Data.Operations.Serialization.MessagePack.yml)** - MessagePack serialization support for operation results
- **[Annium.Data.Operations.Testing](api/base/Data/Annium.Data.Operations.Testing/Annium.Data.Operations.Testing.yml)** - Testing utilities for operation results
- **[Annium.Data.Tables](api/base/Data/Annium.Data.Tables/Annium.Data.Tables.yml)** - Table abstractions and change event management

## Execution

Background execution and workflow management:

- **[Annium.Execution.Background](api/base/Execution/Annium.Execution.Background/Annium.Execution.Background.yml)** - Background task execution with various concurrency models
- **[Annium.Execution.Flow](api/base/Execution/Annium.Execution.Flow/Annium.Execution.Flow.yml)** - Batch and stage execution workflow management

## Extensions

Extended functionality for specialized use cases:

- **[Annium.Extensions.Arguments](api/base/Extensions/Annium.Extensions.Arguments/Annium.Extensions.Arguments.yml)** - Command-line argument parsing and management
- **[Annium.Extensions.CommandLine](api/base/Extensions/Annium.Extensions.CommandLine/Annium.Extensions.CommandLine.yml)** - Command-line interface utilities
- **[Annium.Extensions.Composition](api/base/Extensions/Annium.Extensions.Composition/Annium.Extensions.Composition.yml)** - Object composition and rule-based configuration
- **[Annium.Extensions.Jobs](api/base/Extensions/Annium.Extensions.Jobs/Annium.Extensions.Jobs.yml)** - Job scheduling and interval management
- **[Annium.Extensions.Pooling](api/base/Extensions/Annium.Extensions.Pooling/Annium.Extensions.Pooling.yml)** - Object pooling and caching mechanisms
- **[Annium.Extensions.Reactive](api/base/Extensions/Annium.Extensions.Reactive/System.yml)** - Reactive programming extensions and observer utilities
- **[Annium.Extensions.Shell](api/base/Extensions/Annium.Extensions.Shell/Annium.Extensions.Shell.yml)** - Shell command execution and process management
- **[Annium.Extensions.Validation](api/base/Extensions/Annium.Extensions.Validation/Annium.Extensions.Validation.yml)** - Validation framework with rule builders
- **[Annium.Extensions.Workers](api/base/Extensions/Annium.Extensions.Workers/Annium.Extensions.Workers.yml)** - Background worker management and lifecycle

## Identity & Security

Token-based security and cryptographic utilities:

- **[Annium.Identity.Tokens](api/base/Identity/Annium.Identity.Tokens/Annium.Identity.Tokens.yml)** - Token management and cryptographic algorithm extensions
- **[Annium.Identity.Tokens.Jwt](api/base/Identity/Annium.Identity.Tokens.Jwt/Annium.Identity.Tokens.Jwt.yml)** - JWT token reading, writing, and validation

## Localization

Multi-language support and locale management:

- **[Annium.Localization.Abstractions](api/base/Localization/Annium.Localization.Abstractions/Annium.Localization.Abstractions.yml)** - Core localization abstractions and localizer interface
- **[Annium.Localization.InMemory](api/base/Localization/Annium.Localization.InMemory/Annium.Localization.InMemory.yml)** - In-memory locale storage implementation
- **[Annium.Localization.Yaml](api/base/Localization/Annium.Localization.Yaml/Annium.Localization.Yaml.yml)** - YAML-based locale storage implementation

## Logging

Comprehensive logging infrastructure with multiple providers:

- **[Annium.Logging.Console](api/base/Logging/Annium.Logging.Console/Annium.Logging.Console.yml)** - Console logging provider
- **[Annium.Logging.File](api/base/Logging/Annium.Logging.File/Annium.Logging.File.yml)** - File-based logging with configuration options
- **[Annium.Logging.InMemory](api/base/Logging/Annium.Logging.InMemory/Annium.Logging.InMemory.yml)** - In-memory logging for testing and debugging
- **[Annium.Logging.Microsoft](api/base/Logging/Annium.Logging.Microsoft/Annium.Logging.Microsoft.yml)** - Microsoft.Extensions.Logging integration
- **[Annium.Logging.Shared](api/base/Logging/Annium.Logging.Shared/Annium.Logging.Shared.yml)** - Shared logging infrastructure and handler abstractions
- **[Annium.Logging.Xunit](api/base/Logging/Annium.Logging.Xunit/Annium.Logging.Xunit.yml)** - xUnit test framework logging integration

## Networking

HTTP, Socket, WebSocket, and mail communication:

- **[Annium.Net.Base](api/base/Net/Annium.Net.Base/Annium.Net.Base.yml)** - Base networking utilities and URI management
- **[Annium.Net.Http](api/base/Net/Annium.Net.Http/Annium.Net.Http.yml)** - HTTP client abstractions and request factory
- **[Annium.Net.Mail](api/base/Net/Annium.Net.Mail/Annium.Net.Mail.yml)** - Email service implementation and configuration
- **[Annium.Net.Servers.Sockets](api/base/Net/Annium.Net.Servers.Sockets/Annium.Net.Servers.Sockets.yml)** - Socket server implementation and handler abstractions
- **[Annium.Net.Servers.Web](api/base/Net/Annium.Net.Servers.Web/Annium.Net.Servers.Web.yml)** - Web server implementation with HTTP and WebSocket support
- **[Annium.Net.Sockets](api/base/Net/Annium.Net.Sockets/Annium.Net.Sockets.yml)** - Socket client and server abstractions with connection monitoring
- **[Annium.Net.Types](api/base/Net/Annium.Net.Types/Annium.Net.Types.yml)** - Network type mapping and model transformation utilities
- **[Annium.Net.Types.Serialization.Json](api/base/Net/Annium.Net.Types.Serialization.Json/Annium.Net.Types.Serialization.Json.yml)** - JSON serialization for network types
- **[Annium.Net.WebSockets](api/base/Net/Annium.Net.WebSockets/Annium.Net.WebSockets.yml)** - WebSocket client and server implementation with connection monitoring

## Serialization

Multiple serialization format support:

- **[Annium.Serialization.Abstractions](api/base/Serialization/Annium.Serialization.Abstractions/Annium.Serialization.Abstractions.yml)** - Core serialization abstractions and configuration
- **[Annium.Serialization.BinaryString](api/base/Serialization/Annium.Serialization.BinaryString/Annium.Serialization.BinaryString.yml)** - Binary string serialization support
- **[Annium.Serialization.Json](api/base/Serialization/Annium.Serialization.Json/Annium.Serialization.Json.yml)** - JSON serialization with System.Text.Json integration
- **[Annium.Serialization.MessagePack](api/base/Serialization/Annium.Serialization.MessagePack/Annium.Serialization.MessagePack.yml)** - MessagePack binary serialization support
- **[Annium.Serialization.Yaml](api/base/Serialization/Annium.Serialization.Yaml/Annium.Serialization.Yaml.yml)** - YAML serialization support

## Integrations

Third-party service integrations:

- **[Annium.Graylog.Logging](api/integrations/Graylog/Annium.Graylog.Logging/Annium.Graylog.Logging.yml)** - Graylog logging integration
- **[Annium.NodaTime.Extensions](api/integrations/NodaTime/Annium.NodaTime.Extensions/Annium.NodaTime.Extensions.yml)** - NodaTime library extensions and utilities
- **[Annium.NodaTime.Serialization.Json](api/integrations/NodaTime/Annium.NodaTime.Serialization.Json/Annium.NodaTime.Serialization.Json.yml)** - JSON serialization support for NodaTime types
- **[Annium.Seq.Logging](api/integrations/Seq/Annium.Seq.Logging/Annium.Seq.Logging.yml)** - Seq structured logging integration

## Getting Started

Each component is designed to work independently while providing seamless integration when used together. Explore the API documentation for detailed information about each module.

## Key Features

- **Modular Design** - Use only what you need
- **Result Pattern** - Structured error handling without exceptions
- **Service Container** - Abstraction over Microsoft.Extensions.DI
- **Testing Framework** - Custom assertions and test utilities
- **Code Quality** - Built-in analyzers and formatting rules

## Contributing

If you find any issues or have suggestions for improving the documentation, please feel free to contribute by submitting a pull request.