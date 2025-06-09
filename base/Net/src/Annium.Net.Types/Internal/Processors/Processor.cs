using System.Collections.Generic;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Main processor that coordinates type processing by delegating to specific processor implementations.
/// </summary>
internal class Processor : ILogSubject
{
    /// <summary>
    /// Gets the logger for this processor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The collection of specific processors to delegate to.
    /// </summary>
    private readonly IEnumerable<IProcessor> _processors;

    /// <summary>
    /// Initializes a new instance of the Processor with the specified processors and logger.
    /// </summary>
    /// <param name="processors">The collection of processors to delegate to</param>
    /// <param name="logger">The logger to use for this processor</param>
    public Processor(IEnumerable<IProcessor> processors, ILogger logger)
    {
        Logger = logger;
        _processors = processors;
    }

    /// <summary>
    /// Processes a contextual type using its inherent nullability information.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="ctx">The processing context</param>
    public void Process(ContextualType type, IProcessingContext ctx) => Process(type, type.Nullability, ctx);

    /// <summary>
    /// Processes a contextual type with explicit nullability information by delegating to appropriate processors.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    public void Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        this.Trace<string>("Process {type}", type.FriendlyName());

        foreach (var processor in _processors)
        {
            var result = processor.Process(type, nullability, ctx);
            if (!result)
                continue;

            this.Trace<string, string>(
                "Processed {type} via {processorType}",
                type.FriendlyName(),
                processor.GetType().FriendlyName()
            );
            return;
        }
    }
}
