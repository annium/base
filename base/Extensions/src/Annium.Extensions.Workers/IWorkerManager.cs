using System;

namespace Annium.Extensions.Workers;

public interface IWorkerManager<TData>
    where TData : IEquatable<TData>
{
    void Start(TData key);
    void Stop(TData key);
}