using System;

namespace Annium.Extensions.Pooling.Internal.Storages;

internal interface IStorage<T> : IDisposable
{
    int Capacity { get; }
    int Free { get; }
    int Used { get; }
    void Add(T item);
    T Get();
    void Return(T item);
}