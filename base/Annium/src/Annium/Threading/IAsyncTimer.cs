using System;

namespace Annium.Threading;

public interface IAsyncTimer : IDisposable
{
    void Change(int dueTime, int period);
}
