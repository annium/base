using System;

namespace Annium.Core.Mapper.Internal.DependencyInjection;

internal class ProfileType
{
    public Type Type { get; }

    public ProfileType(Type type)
    {
        Type = type;
    }
}