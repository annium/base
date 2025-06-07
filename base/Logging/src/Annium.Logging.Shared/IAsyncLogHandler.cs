using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Logging.Shared;

/// <summary>
/// Interface for asynchronous log message handlers
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
public interface IAsyncLogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// Handles a batch of log messages asynchronously
    /// </summary>
    /// <param name="messages">The log messages to handle</param>
    /// <returns>A task representing the handling operation</returns>
    ValueTask HandleAsync(IReadOnlyList<LogMessage<TContext>> messages);
}
