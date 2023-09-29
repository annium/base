using System.Runtime.CompilerServices;

namespace Annium.Threading;

public interface IAwaiter<T> : ICriticalNotifyCompletion
{
    bool IsCompleted { get; }
    T GetResult();
}

public interface IAwaiter : ICriticalNotifyCompletion
{
    bool IsCompleted { get; }
    void GetResult();
}