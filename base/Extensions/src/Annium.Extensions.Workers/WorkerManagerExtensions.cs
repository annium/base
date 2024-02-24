using System;

namespace Annium.Extensions.Workers;

public static class WorkerManagerExtensions
{
    public static void SetState<TData>(this IWorkerManager<TData> manager, TData key, bool isActive)
        where TData : IEquatable<TData>
    {
        if (isActive)
            manager.StartAsync(key);
        else
            manager.StopAsync(key);
    }

    public static void SetStateWithKeyUpdate<TData>(
        this IWorkerManager<TData> manager,
        TData oldKey,
        TData newKey,
        bool isActive
    )
        where TData : IEquatable<TData>
    {
        if (newKey.Equals(oldKey))
            manager.SetState(newKey, isActive);
        else
        {
            manager.StopAsync(oldKey);
            if (isActive)
                manager.StartAsync(newKey);
        }
    }
}
