using System.Collections.Generic;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

/// <summary>
/// Interface for type models that support generic type parameters and field definitions.
/// Extends IModel with collections for generic arguments and field models.
/// </summary>
public interface IGenericModel : IModel
{
    /// <summary>
    /// The generic type arguments for this model, if any.
    /// Empty for non-generic types.
    /// </summary>
    IReadOnlyList<IRef> Args { get; }

    /// <summary>
    /// The field definitions for this type model.
    /// Includes properties and fields that are part of the type's structure.
    /// </summary>
    IReadOnlyList<FieldModel> Fields { get; }
}
