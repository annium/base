using System.Collections.Generic;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types;

/// <summary>
/// Primary interface for mapping .NET types to type models and references.
/// Provides functionality to convert runtime types into serializable models and obtain type references.
/// </summary>
public interface IModelMapper
{
    /// <summary>
    /// Maps a contextual type to a type reference, processing all dependent types as needed.
    /// This method analyzes the type structure and creates appropriate models for complex types.
    /// </summary>
    /// <param name="type">The contextual type to map, containing type information and nullability context</param>
    /// <returns>A type reference representing the mapped type</returns>
    IRef Map(ContextualType type);

    /// <summary>
    /// Gets all models that have been created during the mapping process.
    /// This includes all complex types (structs, interfaces, enums) that were processed.
    /// </summary>
    /// <returns>A read-only collection of all generated type models</returns>
    IReadOnlyCollection<IModel> GetModels();
}
