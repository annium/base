namespace Annium.Core.Mapper.Internal.DependencyInjection;

/// <summary>
/// Wrapper for profile instances used in dependency injection
/// </summary>
internal class ProfileInstance
{
    /// <summary>
    /// Gets the profile instance
    /// </summary>
    public Profile Instance { get; }

    /// <summary>
    /// Initializes a new instance of the ProfileInstance class
    /// </summary>
    /// <param name="instance">The profile instance to wrap</param>
    public ProfileInstance(Profile instance)
    {
        Instance = instance;
    }
}
