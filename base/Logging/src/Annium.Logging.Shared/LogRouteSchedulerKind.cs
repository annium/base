namespace Annium.Logging.Shared;

/// <summary>
/// Selects the scheduler that runs an <see cref="ILogHandler{TContext}"/>.
/// Used to override the default selection (which picks <see cref="Background"/>
/// for handlers derived from <see cref="BufferingLogHandler{TContext}"/> and
/// <see cref="Immediate"/> for everything else).
/// </summary>
public enum LogRouteSchedulerKind
{
    /// <summary>
    /// Dispatch via the immediate scheduler — single-message batches, synchronous fan-out.
    /// </summary>
    Immediate,

    /// <summary>
    /// Dispatch via the background scheduler — buffered batches on a pump task.
    /// </summary>
    Background,
}
