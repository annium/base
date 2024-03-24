using System;
using Annium.Logging.Shared;

namespace Annium.Seq.Logging;

public record SeqConfiguration : LogRouteConfiguration
{
    public Uri Endpoint { get; init; } = default!;
    public string ApiKey { get; init; } = string.Empty;
    public string Project { get; init; } = string.Empty;
}
