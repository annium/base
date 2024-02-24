using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Workers;

public interface IWorker<TKey> : IAsyncDisposable
    where TKey : IEquatable<TKey>
{
    ValueTask InitAsync(TKey key);
}
