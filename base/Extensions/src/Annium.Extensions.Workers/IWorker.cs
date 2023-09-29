using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Workers;

public interface IWorker<TData>
    where TData : IEquatable<TData>
{
    ValueTask RunAsync(TData key, CancellationToken ct);
}