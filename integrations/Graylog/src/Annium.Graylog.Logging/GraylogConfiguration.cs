using System;
using Annium.Logging.Shared;

namespace Annium.Graylog.Logging;

/// <summary>
/// Configuration settings for Graylog logging integration that defines connection parameters and project identification for GELF message transmission.
/// </summary>
public record GraylogConfiguration : LogRouteConfiguration
{
    /// <summary>
    /// Gets a value indicating whether Graylog logging is enabled. When disabled, no log messages will be sent to Graylog.
    /// </summary>
    public bool IsEnabled { get; init; } = default!;
    
    /// <summary>
    /// Gets the Graylog server endpoint URI where GELF messages will be transmitted via HTTP POST requests.
    /// </summary>
    public Uri Endpoint { get; init; } = default!;
    
    /// <summary>
    /// Gets the project name used as the host identifier in GELF messages for grouping and filtering logs within Graylog.
    /// </summary>
    public string Project { get; init; } = string.Empty;
}
