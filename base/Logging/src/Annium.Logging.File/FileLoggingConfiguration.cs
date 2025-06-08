using System;
using Annium.Logging.Shared;

namespace Annium.Logging.File;

/// <summary>
/// Configuration for file-based logging including file path resolution.
/// Extends base log route configuration with file-specific settings.
/// </summary>
/// <typeparam name="TContext">The type of the log context</typeparam>
public record FileLoggingConfiguration<TContext> : LogRouteConfiguration
    where TContext : class
{
    /// <summary>
    /// Gets or sets the function to determine the file path for each log message.
    /// </summary>
    public Func<LogMessage<TContext>, string>? GetFile { get; set; }
}
