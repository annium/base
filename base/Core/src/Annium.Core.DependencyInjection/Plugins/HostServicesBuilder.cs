using Annium.Core.DependencyInjection.Internal.Packs;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// This is emulation class for compatibility with extensions, expecting HostBuilder pattern implementation
/// </summary>
/// <typeparam name="TServicePack"></typeparam>
public class HostServicesBuilder<TServicePack>
    where TServicePack : ServicePackBase, new()
{
    /// <summary>
    /// Builds a host services provider with the specified service pack
    /// </summary>
    /// <returns>The built host services provider</returns>
    public HostServicesProvider Build()
    {
        ServiceProviderBuilder builder = new();
        builder.UseServicePack<TServicePack>();

        return new HostServicesProvider(builder.Build());
    }
}
