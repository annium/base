namespace Annium.Core.Runtime.Time;

public interface ITimeProviderSwitcher
{
    void UseRealTime();
    void UseRelativeTime();
    void UseManagedTime();
}