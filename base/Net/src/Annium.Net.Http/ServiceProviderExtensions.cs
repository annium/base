using System;
using Annium.Core.DependencyInjection;

namespace Annium.Net.Http;

public static class ServiceProviderExtensions
{
    public static IHttpRequestFactory ResolveHttpRequestFactory(this IServiceProvider provider)
    {
        return provider.Resolve<IHttpRequestFactory>();
    }

    public static IHttpRequestFactory ResolveHttpRequestFactory(this IServiceProvider provider, string key)
    {
        return provider.ResolveKeyed<IHttpRequestFactory>(key);
    }
}
