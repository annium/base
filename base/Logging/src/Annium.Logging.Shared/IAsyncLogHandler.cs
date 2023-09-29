using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Logging.Shared;

public interface IAsyncLogHandler<TContext>
    where TContext : class, ILogContext
{
    ValueTask Handle(IReadOnlyCollection<LogMessage<TContext>> messages);
}