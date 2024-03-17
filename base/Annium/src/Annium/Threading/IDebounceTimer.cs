using System;

namespace Annium.Threading;

public interface IDebounceTimer : IDisposable, IAsyncDisposable
{
    void Change(int period);
    void Request();
}
