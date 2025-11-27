using System;
using System.Net.Http;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Http.Internal;
using Constants = Annium.Net.Http.Internal.Constants;
using Serializer = Annium.Net.Http.Internal.Serializer;

namespace Annium.Net.Http;

/// <summary>
/// Extensions for configuring HTTP request factory services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds HTTP request factory services to the container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="isDefault">Whether to register as default services</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddHttpRequestFactory(this IServiceContainer container, bool isDefault = false)
    {
        return container.AddHttpRequestFactory(Constants.DefaultKey, (_, _) => new HttpClient(), isDefault);
    }

    /// <summary>
    /// Adds HTTP request factory services to the container with a specific key
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="key">The service key</param>
    /// <param name="isDefault">Whether to register as default services</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddHttpRequestFactory(
        this IServiceContainer container,
        string key,
        bool isDefault = false
    )
    {
        return container.AddHttpRequestFactory(key, (_, _) => new HttpClient(), isDefault);
    }

    /// <summary>
    /// Adds HTTP request factory services to the container with a specific key
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="getHttpClient">HttpClient factory function</param>
    /// <param name="isDefault">Whether to register as default services</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddHttpRequestFactory(
        this IServiceContainer container,
        Func<IServiceProvider, HttpClient> getHttpClient,
        bool isDefault = false
    )
    {
        return container.AddHttpRequestFactory(Constants.DefaultKey, (sp, _) => getHttpClient(sp), isDefault);
    }

    /// <summary>
    /// Adds HTTP request factory services to the container with a specific key
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="key">The service key</param>
    /// <param name="getHttpClient">HttpClient factory function</param>
    /// <param name="isDefault">Whether to register as default services</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddHttpRequestFactory(
        this IServiceContainer container,
        string key,
        Func<IServiceProvider, string, HttpClient> getHttpClient,
        bool isDefault = false
    )
    {
        container.Add(static (sp, key) => new Serializer(sp, key.ToString().NotNull())).AsKeyedSelf(key).Singleton();

        container
            .Add((sp, k) => new HttpClientWrapper(getHttpClient(sp, k.ToString().NotNull())))
            .AsKeyedSelf(key)
            .Singleton();

        container
            .Add<IHttpRequestFactory>(
                static (sp, key) =>
                {
                    var client = sp.ResolveKeyed<HttpClientWrapper>(key);
                    var serializer = sp.ResolveKeyed<Serializer>(key);
                    var logger = sp.Resolve<ILogger>();

                    return new HttpRequestFactory(client.HttpClient, serializer, logger);
                }
            )
            .AsKeyed<IHttpRequestFactory>(key)
            .Singleton();

        if (!isDefault)
            return container;

        // default serializer+factory
        container.Add(sp => sp.ResolveKeyed<Serializer>(key)).As<Serializer>().Singleton();
        container.Add(sp => sp.ResolveKeyed<IHttpRequestFactory>(key)).As<IHttpRequestFactory>().Singleton();

        return container;
    }
}

/// <summary>
/// HttpClientWrapper for being accessible in DI
/// </summary>
/// <param name="HttpClient"></param>
file record HttpClientWrapper(HttpClient HttpClient) : IDisposable
{
    /// <summary>
    /// Disposes wrapped HttpClient
    /// </summary>
    public void Dispose()
    {
        HttpClient.Dispose();
    }
}
