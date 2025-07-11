using System;

namespace Annium.Core.Mapper.Internal.Profiles;

/// <summary>
/// Generic profile that provides string conversion mappings for enum types
/// </summary>
/// <typeparam name="T">The enum type</typeparam>
internal class EnumProfile<T> : Profile
    where T : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the EnumProfile class
    /// </summary>
    public EnumProfile()
    {
        Map<T, string>(x => x.ToString());
        Map<string, T>(x => x.ParseEnum<T>());
    }
}
