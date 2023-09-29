using System;

namespace Annium.Extensions.Pooling;

public interface IObjectPool<T> : IDisposable
{
    T Get();
    void Return(T item);
}