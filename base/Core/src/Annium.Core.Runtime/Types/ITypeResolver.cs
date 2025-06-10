using System;
using System.Collections.Generic;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Interface for resolving types based on a given type criteria
/// </summary>
public interface ITypeResolver
{
    /// <summary>
    /// Resolves a collection of types based on the specified type criteria
    /// </summary>
    /// <param name="type">The type to use as resolution criteria</param>
    /// <returns>A collection of resolved types</returns>
    IReadOnlyCollection<Type> ResolveType(Type type);
}
