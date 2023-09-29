using System.Linq;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Net.Http.Internal;
using Annium.Serialization.Abstractions;
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

    public static IServiceContainer AddHttpRequestFactory(this IServiceContainer container, string key, bool isDefault = false)
    {
        container.Add<Serializer>(sp =>
        {
            var serializers = sp.Resolve<IIndex<SerializerKey, ISerializer<string>>>()
                .Where(x => x.Key.Key == key)
                .ToIndex(x => x.Key.MediaType, x => x.Value);

            return new Serializer(serializers);
        }).AsKeyed<Serializer, string>(key).Singleton();

        container.Add<IHttpRequestFactory>(sp =>
        {
            var serializer = sp.Resolve<IIndex<string, Serializer>>()[key];
            var logger = sp.Resolve<ILogger>();

            return new HttpRequestFactory(serializer, logger);
        }).AsKeyed<IHttpRequestFactory, string>(key).Singleton();

        if (!isDefault)
            return container;

        // default serializer+factory
        container.Add(sp => sp.Resolve<IIndex<string, Serializer>>()[key]).As<Serializer>().Singleton();
        container.Add(sp => sp.Resolve<IIndex<string, IHttpRequestFactory>>()[key]).As<IHttpRequestFactory>().Singleton();

        return container;
    }
}