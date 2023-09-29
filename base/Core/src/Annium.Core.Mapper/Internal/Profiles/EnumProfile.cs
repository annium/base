using System;

namespace Annium.Core.Mapper.Internal.Profiles;

internal class EnumProfile<T> : Profile
    where T : struct, Enum
{
    public EnumProfile()
    {
        Map<T, string>(x => x.ToString());
        Map<string, T>(x => x.ParseEnum<T>());
    }
}