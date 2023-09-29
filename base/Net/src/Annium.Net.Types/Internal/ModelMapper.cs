using System.Collections.Generic;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal class ModelMapper : IModelMapper
{
    private readonly IMapperProcessingContext _ctx;

    public ModelMapper(IMapperProcessingContext ctx)
    {
        _ctx = ctx;
    }

    public IRef Map(ContextualType type)
    {
        _ctx.Process(type);

        return _ctx.GetRef(type);
    }

    public IReadOnlyCollection<IModel> GetModels() => _ctx.GetModels();
}