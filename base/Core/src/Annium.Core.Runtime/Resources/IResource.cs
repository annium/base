using System;

namespace Annium.Core.Runtime.Resources;

public interface IResource
{
    string Name { get; }
    ReadOnlyMemory<byte> Content { get; }

    void Deconstruct(
        out string name,
        out ReadOnlyMemory<byte> content
    );
}