using System;
using System.Collections.Generic;

namespace Annium.Data.Tables;

public interface ITableView<out T> : IReadOnlyCollection<T>, IObservable<IChangeEvent<T>>, IAsyncDisposable
{
}