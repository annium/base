using System;
using System.Collections.Generic;

namespace Annium.Data.Tables;

/// <summary>
/// Interface for the view capabilities of a table, providing read-only access and change notifications
/// </summary>
/// <typeparam name="T">The type of items in the table view</typeparam>
public interface ITableView<T> : IReadOnlyCollection<T>, IObservable<ChangeEvent<T>>, IAsyncDisposable
    where T : notnull;
