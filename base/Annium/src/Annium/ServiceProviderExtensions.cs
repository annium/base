using System;

namespace Annium;

public static class ServiceProviderExtensions
{
    public static void Inject<T>(this IServiceProvider sp, T value)
        where T : class
    {
        sp.GetService(typeof(Injected<T>)).NotNull().CastTo<Injected<T>>().Init(value);
    }
}
