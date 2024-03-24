using System;
using Annium.Logging.Shared;

namespace Annium.Graylog.Logging;

public record GraylogConfiguration : LogRouteConfiguration
{
    public Uri Endpoint { get; init; } = default!;
    public string Project { get; init; } = string.Empty;
}
