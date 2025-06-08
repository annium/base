using System;
using Annium.Core.Runtime.Resources;

namespace Annium.Core.Runtime.Internal.Resources;

/// <summary>
/// Internal implementation of a resource containing name and binary content
/// </summary>
internal class Resource : IResource
{
    /// <summary>
    /// The name of the resource
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The binary content of the resource
    /// </summary>
    public ReadOnlyMemory<byte> Content { get; }

    /// <summary>
    /// Initializes a new instance of Resource with specified name and content
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <param name="content">The binary content of the resource</param>
    public Resource(string name, ReadOnlyMemory<byte> content)
    {
        Name = name;
        Content = content;
    }

    /// <summary>
    /// Deconstructs the resource into its name and content components
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <param name="content">The binary content of the resource</param>
    public void Deconstruct(out string name, out ReadOnlyMemory<byte> content)
    {
        name = Name;
        content = Content;
    }
}
