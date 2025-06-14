using System;
using Annium.Core.DependencyInjection;

namespace Annium.Net.Http;

/// <summary>
/// Extensions for resolving HTTP request factory services
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Resolves the default HTTP request factory from the service provider
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <returns>The HTTP request factory</returns>
    public static IHttpRequestFactory ResolveHttpRequestFactory(this IServiceProvider provider)
    {
        return provider.Resolve<IHttpRequestFactory>();
    }

    /// <summary>
    /// Resolves a keyed HTTP request factory from the service provider
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="key">The service key</param>
    /// <returns>The HTTP request factory</returns>
    public static IHttpRequestFactory ResolveHttpRequestFactory(this IServiceProvider provider, string key)
    {
        return provider.ResolveKeyed<IHttpRequestFactory>(key);
    }
}
