using Annium.Logging;

namespace Annium.Data.Tables.Internal;

/// <summary>
/// Internal implementation of a table factory for creating table builders.
/// </summary>
internal class TableFactory : ITableFactory
{
    /// <summary>
    /// Logger instance used for creating tables.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the TableFactory class.
    /// </summary>
    /// <param name="logger">Logger instance for creating tables.</param>
    public TableFactory(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new table builder for the specified item type.
    /// </summary>
    /// <typeparam name="T">The type of items that will be stored in the table.</typeparam>
    /// <returns>A new table builder instance.</returns>
    public ITableBuilder<T> New<T>()
        where T : notnull
    {
        return new TableBuilder<T>(_logger);
    }
}
