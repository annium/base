using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Execution.Background;

public interface IExecutor : IAsyncDisposable
{
    bool IsAvailable { get; }
    bool Schedule(Action task);
    bool Schedule(Action<CancellationToken> task);
    bool Schedule(Func<ValueTask> task);
    bool Schedule(Func<CancellationToken, ValueTask> task);
    IExecutor Start(CancellationToken ct = default);
}
