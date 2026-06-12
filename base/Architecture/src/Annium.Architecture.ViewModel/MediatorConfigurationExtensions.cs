using Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;
using Annium.Core.Mediator;

namespace Annium.Architecture.ViewModel;

/// <summary>
/// Extension methods for configuring view model mapping handlers in the mediator
/// </summary>
public static class MediatorConfigurationExtensions
{
    /// <summary>
    /// Adds view model mapping pipe handlers to the mediator configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ordering matters. The response-side mappers expect <c>IStatusResult&lt;OperationStatus, TData&gt;</c>
    /// to flow in from upstream, so this call MUST be made BEFORE
    /// <c>AddHttpStatusPipeHandler</c> (which strips the status and throws on non-Ok). The
    /// canonical pipeline order is:
    /// </para>
    /// <para>
    /// <c>Logging → Exception → Validation → Composition → AddViewMappingHandlers → [final handler]</c>,
    /// then <c>AddHttpStatusPipeHandler</c> at the outermost position when the result needs to
    /// surface as an HTTP exception.
    /// </para>
    /// </remarks>
    /// <param name="cfg">The mediator configuration to extend</param>
    /// <returns>The updated mediator configuration</returns>
    public static MediatorConfiguration AddViewMappingHandlers(this MediatorConfiguration cfg)
    {
        cfg.AddHandler(typeof(MappingEnumerablePipeHandler<,,>));
        cfg.AddHandler(typeof(MappingSinglePipeHandler<,,>));
        cfg.AddHandler(typeof(Internal.PipeHandlers.Response.MappingEnumerablePipeHandler<,,>));
        cfg.AddHandler(typeof(Internal.PipeHandlers.Response.MappingSinglePipeHandler<,,>));

        return cfg;
    }
}
