using System.Reflection;
using System.Runtime.CompilerServices;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;
using Annium.Testing.Assertions.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

public static class ShallowEqualityExtensions
{
    public static void IsEqual<T, TD>(
        this T value,
        TD data,
        string? message = null,
        IMapper? mapper = default,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
    {
        if (!value.IsShallowEqual(data, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} is not shallow equal to {data.Wrap(dataEx)}");
    }

    public static void IsNotEqual<T, TD>(
        this T value,
        TD data,
        string? message = null,
        IMapper? mapper = default,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
    {
        if (value.IsShallowEqual(data, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} is shallow equal to {data.Wrap(dataEx)}");
    }
}