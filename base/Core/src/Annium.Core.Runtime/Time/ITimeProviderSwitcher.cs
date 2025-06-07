namespace Annium.Core.Runtime.Time;

/// <summary>
/// Interface for switching between different time provider implementations at runtime
/// </summary>
public interface ITimeProviderSwitcher
{
    /// <summary>
    /// Switches to using real system time
    /// </summary>
    void UseRealTime();

    /// <summary>
    /// Switches to using relative time
    /// </summary>
    void UseRelativeTime();

    /// <summary>
    /// Switches to using managed time
    /// </summary>
    void UseManagedTime();
}
