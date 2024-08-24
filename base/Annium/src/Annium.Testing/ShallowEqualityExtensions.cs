using System.Reflection;
using System.Runtime.CompilerServices;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;

namespace Annium.Testing;

public static class ShallowEqualityExtensions
{
    public static T IsEqual<T, TD>(
        this T value,
        TD data,
        string? message = null,
        IMapper? mapper = default,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(data))] string dataEx = default!
    )
    {
        if (!value.IsShallowEqual(data, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(
                message
                    ?? $"{value.WrapWithExpression(valueEx)} is not shallow equal to {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    public static T IsNotEqual<T, TD>(
        this T value,
        TD data,
        string? message = null,
        IMapper? mapper = default,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(data))] string dataEx = default!
    )
    {
        if (value.IsShallowEqual(data, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} is shallow equal to {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }
}
