using System;

namespace Annium.Threading;

public interface IDebounceTimer : IDisposable
{
    void Change(int period);
    void Request();
}
