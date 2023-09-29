using Annium.Logging;

namespace Annium.Data.Tables.Internal;

internal class TableFactory : ITableFactory
{
    private readonly ILogger _logger;

    public TableFactory(
        ILogger logger
    )
    {
        _logger = logger;
    }

    public ITableBuilder<T> New<T>()
        where T : notnull
    {
        return new TableBuilder<T>(_logger);
    }
}