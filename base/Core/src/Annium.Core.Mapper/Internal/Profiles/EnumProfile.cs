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

internal class EnumProfile<T1, T2> : Profile
    where T1 : struct, Enum
    where T2 : struct, Enum
{
    public EnumProfile()
    {
        Map<T1, T2>(x => x.ToString().ParseEnum<T2>());
        Map<T2, T1>(x => x.ToString().ParseEnum<T1>());
    }
}
