using System;

namespace Annium.Core.Mapper.Internal.DependencyInjection;

/// <summary>
/// Wrapper for profile types used in dependency injection
/// </summary>
internal class ProfileType
{
    /// <summary>
    /// Gets the profile type
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Initializes a new instance of the ProfileType class
    /// </summary>
    /// <param name="type">The profile type to wrap</param>
    public ProfileType(Type type)
    {
        Type = type;
    }
}
