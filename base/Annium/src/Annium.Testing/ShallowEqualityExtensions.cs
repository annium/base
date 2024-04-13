using System.Reflection;
using System.Runtime.CompilerServices;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;

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
            throw new AssertionFailedException(
                message
                    ?? $"{value.WrapWithExpression(valueEx)} is not shallow equal to {data.WrapWithExpression(dataEx)}"
            );
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
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} is shallow equal to {data.WrapWithExpression(dataEx)}"
            );
    }
}
