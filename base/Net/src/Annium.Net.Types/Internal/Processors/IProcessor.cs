using Annium.Logging;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal interface IProcessor : ILogSubject
{
    // int Order { get; }
    bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx);
}