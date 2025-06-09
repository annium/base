using Annium.Logging;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Interface for type processors that handle specific scenarios during type mapping.
/// Processors determine whether they can handle a type and perform the necessary processing.
/// </summary>
internal interface IProcessor : ILogSubject
{
    // int Order { get; }
    /// <summary>
    /// Processes the specified contextual type if this processor can handle it.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability context</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>True if the processor handled the type, false otherwise</returns>
    bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx);
}
