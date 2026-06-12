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
    // must stay public: closed generic EnumProfile<T> instances are DI-activated by reflection
    // (ActivatorUtilities) for autoloaded [AutoMapped] enum types, which requires a public constructor
    public EnumProfile()
    {
        Map<T, string>(x => x.ToString());
        Map<string, T>(x => x.ParseEnum<T>());
    }
}
