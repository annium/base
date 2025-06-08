using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Annium.Core.Mapper;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Models.Extensions;

/// <summary>
/// Extension methods for shallow equality comparison.
/// </summary>
public static partial class IsShallowEqualExtensions
{
    /// <summary>
    /// Lock object for thread-safe access to comparer caches.
    /// </summary>
    private static readonly Lock _locker = new();

    /// <summary>
    /// Set of types currently being processed to prevent circular dependencies.
    /// </summary>
    private static readonly HashSet<Type> _comparersInProgress = new();

    /// <summary>
    /// Cache of compiled comparer delegates for each type.
    /// </summary>
    private static readonly IDictionary<Type, Delegate> _comparers = new Dictionary<Type, Delegate>();

    /// <summary>
    /// Cache of raw lambda expressions for each type before compilation.
    /// </summary>
    private static readonly IDictionary<Type, LambdaExpression> _rawComparers =
        new Dictionary<Type, LambdaExpression>();

    /// <summary>
    /// Determines whether two values are shallowly equal using the default mapper
    /// </summary>
    /// <typeparam name="T">The type of the first value</typeparam>
    /// <typeparam name="TD">The type of the second value</typeparam>
    /// <param name="value">The first value to compare</param>
    /// <param name="data">The second value to compare</param>
    /// <returns>True if the values are shallowly equal</returns>
    public static bool IsShallowEqual<T, TD>(this T value, TD data)
    {
        var mapper = Mapper.GetFor(Assembly.GetCallingAssembly());

        return value.IsShallowEqual(data, mapper);
    }

    /// <summary>
    /// Determines whether two values are shallowly equal using the specified mapper
    /// </summary>
    /// <typeparam name="T">The type of the first value</typeparam>
    /// <typeparam name="TD">The type of the second value</typeparam>
    /// <param name="value">The first value to compare</param>
    /// <param name="data">The second value to compare</param>
    /// <param name="mapper">The mapper to use for type conversions</param>
    /// <returns>True if the values are shallowly equal</returns>
    public static bool IsShallowEqual<T, TD>(this T value, TD data, IMapper mapper)
    {
        var type = typeof(TD);

        if (type.IsClass || type.IsInterface)
        {
            // if data is null, simply compare to null
            if (data is null)
                return value is null;

            // if data is not null, but value - is, return false
            if (value is null)
                return false;

            // for reference equality - return true
            if (ReferenceEquals(value, data))
                return true;
        }

        // if compared as objects - need to resolve target real type
        if (type == typeof(object))
            type = data?.GetType() ?? typeof(object);

        // as far as base object class has no properties, consider objects to be shallowly equal
        if (type == typeof(object))
            return true;

        var comparable = mapper.Map(value!, type);

        lock (_locker)
        {
            try
            {
                ResolveComparer(type, mapper);
            }
            finally
            {
                _comparersInProgress.Clear();
            }
        }

        var comparer = _comparers[type];
        // var str = RawComparers[type].ToReadableString();

        try
        {
            return (bool)comparer.DynamicInvoke(comparable, data)!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    /// <summary>
    /// Resolves or builds a comparer for the specified type.
    /// </summary>
    /// <param name="type">The type to resolve a comparer for.</param>
    /// <param name="mapper">The mapper to use for type conversions.</param>
    /// <returns>The lambda expression comparer for the type.</returns>
    private static LambdaExpression ResolveComparer(Type type, IMapper mapper)
    {
        if (_rawComparers.TryGetValue(type, out var comparer))
            return comparer;

        if (!_comparersInProgress.Add(type))
            return BuildExtensionCallComparer(type, mapper);

        comparer = _rawComparers[type] = BuildComparer(type, mapper);
        _comparersInProgress.Remove(type);

        _comparers[type] = comparer.Compile();

        return comparer;
    }
}
