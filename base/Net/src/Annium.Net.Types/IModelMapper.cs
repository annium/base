using System.Collections.Generic;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types;

public interface IModelMapper
{
    IRef Map(ContextualType type);
    IReadOnlyCollection<IModel> GetModels();
}