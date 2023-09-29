using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Jobs;

public interface IScheduler
{
    IDisposable Schedule(Func<Task> handler, string interval);
}