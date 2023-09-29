using System.Collections.Generic;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

public interface IGenericModel : IModel
{
    IReadOnlyList<IRef> Args { get; }
    IReadOnlyList<FieldModel> Fields { get; }
}