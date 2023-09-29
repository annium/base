using System;
using Annium.Logging.Shared;

namespace Annium.Logging.File;

public record FileLoggingConfiguration<TContext> : LogRouteConfiguration
    where TContext : class, ILogContext
{
    public Func<LogMessage<TContext>, string>? GetFile { get; set; }
}