using System;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Exception thrown when type resolution fails during runtime type operations
/// </summary>
public class TypeResolutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of TypeResolutionException with source and target types
    /// </summary>
    /// <param name="src">The source type that failed to resolve</param>
    /// <param name="tgt">The target type that could not be resolved to</param>
    /// <param name="messages">Additional error messages describing the failure</param>
    public TypeResolutionException(Type src, Type tgt, params string[] messages)
        : base($"Can't convert {src.FullName} -> {tgt.FullName}. {string.Join(Environment.NewLine, messages)}") { }
}
