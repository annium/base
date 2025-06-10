using System;
using Annium.Logging.Shared;

namespace Annium.Seq.Logging;

/// <summary>
/// Configuration record for Seq logging integration containing connection details and project settings.
/// Inherits from LogRouteConfiguration to include buffering and filtering options.
/// </summary>
public record SeqConfiguration : LogRouteConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether Seq logging is enabled.
    /// When false, the Seq log handler will not be registered in the logging pipeline.
    /// </summary>
    public bool IsEnabled { get; init; } = default!;

    /// <summary>
    /// Gets or sets the base URI of the Seq server where log events will be sent.
    /// Should include the protocol and port (e.g., https://seq.example.com:5341).
    /// </summary>
    public Uri Endpoint { get; init; } = default!;

    /// <summary>
    /// Gets or sets the API key for authenticating with the Seq server.
    /// Used in the X-Seq-ApiKey header for API authentication.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the project identifier to include in CLEF events.
    /// Used as the @p field in Compact Log Event Format to categorize log events by project.
    /// </summary>
    public string Project { get; init; } = string.Empty;
}
