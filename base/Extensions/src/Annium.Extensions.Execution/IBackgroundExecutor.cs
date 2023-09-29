using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution;

public interface IBackgroundExecutor : IAsyncDisposable
{
    bool IsAvailable { get; }
    void Schedule(Action task);
    void Schedule(Action<CancellationToken> task);
    void Schedule(Func<ValueTask> task);
    void Schedule(Func<CancellationToken, ValueTask> task);
    bool TrySchedule(Action task);
    bool TrySchedule(Action<CancellationToken> task);
    bool TrySchedule(Func<ValueTask> task);
    bool TrySchedule(Func<CancellationToken, ValueTask> task);
    void Start(CancellationToken ct = default);
}