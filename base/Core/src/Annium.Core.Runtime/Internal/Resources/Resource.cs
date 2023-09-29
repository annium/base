using System;
using Annium.Core.Runtime.Resources;

namespace Annium.Core.Runtime.Internal.Resources;

internal class Resource : IResource
{
    public string Name { get; }
    public ReadOnlyMemory<byte> Content { get; }

    public Resource(
        string name,
        ReadOnlyMemory<byte> content
    )
    {
        Name = name;
        Content = content;
    }

    public void Deconstruct(
        out string name,
        out ReadOnlyMemory<byte> content
    )
    {
        name = Name;
        content = Content;
    }
}