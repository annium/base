using Annium.Core.DependencyInjection;

namespace Annium.Core.Runtime.Time;

public interface ITimeConfigurationBuilder
{
    ITimeConfigurationBuilder WithRealTime();
    ITimeConfigurationBuilder WithRelativeTime();
    ITimeConfigurationBuilder WithManagedTime();
    IServiceContainer SetDefault();
}