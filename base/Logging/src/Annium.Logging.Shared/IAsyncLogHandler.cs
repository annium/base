using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Logging.Shared;

public interface IAsyncLogHandler<TContext>
    where TContext : class
{
    ValueTask HandleAsync(IReadOnlyCollection<LogMessage<TContext>> messages);
}
