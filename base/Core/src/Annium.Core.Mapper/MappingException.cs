using System;

namespace Annium.Core.Mapper;

/// <summary>
/// Exception thrown when mapping between types fails
/// </summary>
public class MappingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the MappingException class
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <param name="messages">Additional error messages</param>
    public MappingException(Type src, Type tgt, params string[] messages)
        : base($"Can't convert {src.FullName} -> {tgt.FullName}. {string.Join(Environment.NewLine, messages)}") { }
}
