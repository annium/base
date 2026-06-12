namespace Annium.Core.Mapper.Internal.DependencyInjection;

/// <summary>
/// Wrapper for profile instances used in dependency injection
/// </summary>
internal class ProfileInstance
{
    /// <summary>
    /// Gets the profile instance
    /// </summary>
    internal Profile Instance { get; }

    /// <summary>
    /// Initializes a new instance of the ProfileInstance class
    /// </summary>
    /// <param name="instance">The profile instance to wrap</param>
    internal ProfileInstance(Profile instance)
    {
        Instance = instance;
    }
}
