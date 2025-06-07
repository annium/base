using System;

namespace Annium.Core.Mapper;

/// <summary>
/// Delegate for resolving mappings between types
/// </summary>
/// <param name="src">The source type</param>
/// <param name="tgt">The target type</param>
/// <returns>The resolved mapping, or null if no mapping exists</returns>
public delegate Mapping? ResolveMapping(Type src, Type tgt);
