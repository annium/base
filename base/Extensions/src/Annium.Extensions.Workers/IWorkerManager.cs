using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Workers;

public interface IWorkerManager<TData>
    where TData : IEquatable<TData>
{
    Task StartAsync(TData key);
    Task StopAsync(TData key);
}
