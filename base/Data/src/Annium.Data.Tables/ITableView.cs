using System;
using System.Collections.Generic;

namespace Annium.Data.Tables;

public interface ITableView<T> : IReadOnlyCollection<T>, IObservable<ChangeEvent<T>>, IAsyncDisposable
    where T : notnull;
