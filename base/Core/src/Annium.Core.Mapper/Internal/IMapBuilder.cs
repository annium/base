using System;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Internal contract for building and querying compiled mappings between type pairs.
/// </summary>
/// <remarks>
/// The interface intentionally exposes only query operations consumed by <see cref="Internal.Mapper"/>.
/// Profile registration goes through DI (<see cref="MapperRegistration"/>) rather than this contract.
/// </remarks>
internal interface IMapBuilder
{
    /// <summary>
    /// Determines if a mapping exists between the specified types.
    /// </summary>
    /// <param name="src">The source type.</param>
    /// <param name="tgt">The target type.</param>
    /// <returns>True if a mapping exists, otherwise false.</returns>
    bool HasMap(Type src, Type tgt);

    /// <summary>
    /// Gets the compiled mapping delegate between the specified types.
    /// </summary>
    /// <param name="src">The source type.</param>
    /// <param name="tgt">The target type.</param>
    /// <returns>The compiled mapping delegate.</returns>
    Delegate GetMap(Type src, Type tgt);
}
