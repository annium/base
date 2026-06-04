namespace Annium.Core.Runtime.Time;

/// <summary>
/// Interface for switching between different time provider implementations at runtime.
/// Each switch target must have been registered during configuration via the matching
/// <c>ITimeConfigurationBuilder.With*</c> call; switching to an unregistered mode throws
/// a service-resolution exception.
/// </summary>
public interface ITimeProviderSwitcher
{
    /// <summary>
    /// Switches to using real system time. Requires that <c>WithRealTime()</c> was configured;
    /// otherwise throws a service-resolution exception.
    /// </summary>
    void UseRealTime();

    /// <summary>
    /// Switches to using relative time. Requires that <c>WithRelativeTime()</c> was configured;
    /// otherwise throws a service-resolution exception.
    /// </summary>
    void UseRelativeTime();

    /// <summary>
    /// Switches to using managed time. Requires that <c>WithManagedTime()</c> was configured;
    /// otherwise throws a service-resolution exception.
    /// </summary>
    void UseManagedTime();
}
