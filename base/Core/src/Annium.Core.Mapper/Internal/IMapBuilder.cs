using System;

namespace Annium.Core.Mapper.Internal;

public interface IMapBuilder
{
    IMapBuilder AddProfile(Action<Profile> configure);

    IMapBuilder AddProfile<T>() where T : Profile;

    IMapBuilder AddProfile(Type profileType);

    bool HasMap(Type src, Type tgt);

    Delegate GetMap(Type src, Type tgt);
}