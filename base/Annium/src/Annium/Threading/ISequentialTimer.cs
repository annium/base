using System;

namespace Annium.Threading;

public interface ISequentialTimer : IDisposable
{
    void Change(int dueTime, int period);
}
