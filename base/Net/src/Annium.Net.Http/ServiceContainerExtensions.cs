using Annium.Logging;
using Annium.Net.Http;
using Annium.Net.Http.Internal;
using Constants = Annium.Net.Http.Internal.Constants;
using Serializer = Annium.Net.Http.Internal.Serializer;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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
        return container.AddHttpRequestFactory(Constants.DefaultKey, isDefault);
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
        container
            .Add(static (sp, key) => new Serializer(sp, key.ToString().NotNull()))
            .AsKeyed<Serializer>(key)
            .Singleton();

        container
            .Add<IHttpRequestFactory>(
                static (sp, key) =>
                {
                    var serializer = sp.ResolveKeyed<Serializer>(key);
                    var logger = sp.Resolve<ILogger>();

                    return new HttpRequestFactory(serializer, logger);
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
