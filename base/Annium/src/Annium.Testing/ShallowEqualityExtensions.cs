using System.Reflection;
using System.Runtime.CompilerServices;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions.IsShallowEqual;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for asserting shallow equality in tests.
/// </summary>
public static class ShallowEqualityExtensions
{
    /// <summary>
    /// Asserts that the value is shallow equal to the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TD">The type of the data to compare.</typeparam>
    /// <param name="value">The value to compare.</param>
    /// <param name="data">The data to compare against.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="mapper">The mapper to use for comparison. If null, a default mapper is used.</param>
    /// <param name="valueEx">The expression representing the value parameter.</param>
    /// <param name="dataEx">The expression representing the data parameter.</param>
    /// <returns>The original value if the assertion passes.</returns>
    /// <exception cref="AssertionFailedException">Thrown if the value is not shallow equal to the data.</exception>
    public static T IsEqual<T, TD>(
        this T value,
        TD data,
        string? message = null,
        IMapper? mapper = default,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
    )
    {
        if (!value.IsShallowEqual(data, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(
                message
                    ?? $"{value.WrapWithExpression(valueEx)} is not shallow equal to {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    /// <summary>
    /// Asserts that the value is not shallow equal to the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TD">The type of the data to compare.</typeparam>
    /// <param name="value">The value to compare.</param>
    /// <param name="data">The data to compare against.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="mapper">The mapper to use for comparison. If null, a default mapper is used.</param>
    /// <param name="valueEx">The expression representing the value parameter.</param>
    /// <param name="dataEx">The expression representing the data parameter.</param>
    /// <returns>The original value if the assertion passes.</returns>
    /// <exception cref="AssertionFailedException">Thrown if the value is shallow equal to the data.</exception>
    public static T IsNotEqual<T, TD>(
        this T value,
        TD data,
        string? message = null,
        IMapper? mapper = default,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
    )
    {
        if (value.IsShallowEqual(data, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} is shallow equal to {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }
}
