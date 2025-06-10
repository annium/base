using System.Collections.Generic;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

/// <summary>
/// Internal implementation of the model mapper that coordinates type processing and model generation.
/// </summary>
internal class ModelMapper : IModelMapper
{
    /// <summary>
    /// The processing context used for mapping operations.
    /// </summary>
    private readonly IMapperProcessingContext _ctx;

    /// <summary>
    /// Initializes a new instance of the ModelMapper with the specified processing context.
    /// </summary>
    /// <param name="ctx">The processing context to use for mapping operations</param>
    public ModelMapper(IMapperProcessingContext ctx)
    {
        _ctx = ctx;
    }

    /// <summary>
    /// Maps a contextual type to a type reference, processing all dependent types as needed.
    /// </summary>
    /// <param name="type">The contextual type to map</param>
    /// <returns>A type reference representing the mapped type</returns>
    public IRef Map(ContextualType type)
    {
        _ctx.Process(type);

        return _ctx.GetRef(type);
    }

    /// <summary>
    /// Gets all models that have been created during the mapping process.
    /// </summary>
    /// <returns>A read-only collection of all generated type models</returns>
    public IReadOnlyCollection<IModel> GetModels() => _ctx.GetModels();
}
