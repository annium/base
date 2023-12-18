using Annium.Logging;
using Annium.Net.Http;
using Annium.Net.Http.Internal;
using Constants = Annium.Net.Http.Internal.Constants;
using Serializer = Annium.Net.Http.Internal.Serializer;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddHttpRequestFactory(this IServiceContainer container, bool isDefault = false)
    {
        return container.AddHttpRequestFactory(Constants.DefaultKey, isDefault);
    }

    public static IServiceContainer AddHttpRequestFactory(
        this IServiceContainer container,
        string key,
        bool isDefault = false
    )
    {
        container
            .Add<Serializer>(static (sp, key) => new Serializer(sp, key.ToString().NotNull()))
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
