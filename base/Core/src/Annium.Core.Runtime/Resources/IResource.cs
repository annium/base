using System;

namespace Annium.Core.Runtime.Resources;

/// <summary>
/// Interface for a resource containing name and binary content
/// </summary>
public interface IResource
{
    /// <summary>
    /// The name of the resource
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The binary content of the resource
    /// </summary>
    ReadOnlyMemory<byte> Content { get; }

    /// <summary>
    /// Deconstructs the resource into its name and content components
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <param name="content">The binary content of the resource</param>
    void Deconstruct(out string name, out ReadOnlyMemory<byte> content);
}
