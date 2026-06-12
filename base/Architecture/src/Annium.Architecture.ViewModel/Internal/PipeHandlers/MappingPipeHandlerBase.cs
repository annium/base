using Annium.Core.Mapper;
using Annium.Logging;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers;

/// <summary>
/// Base class for view-model mapping pipe handlers, holding the shared mapper and logger.
/// </summary>
internal abstract class MappingPipeHandlerBase : ILogSubject
{
    /// <summary>
    /// Gets the logger for this pipe handler.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The mapper instance used to map between view-model and underlying types.
    /// </summary>
    protected readonly IMapper Mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingPipeHandlerBase"/> class.
    /// </summary>
    /// <param name="mapper">The mapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    protected MappingPipeHandlerBase(IMapper mapper, ILogger logger)
    {
        Mapper = mapper;
        Logger = logger;
    }
}
