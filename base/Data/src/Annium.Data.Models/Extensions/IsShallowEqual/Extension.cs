using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Annium.Core.Mapper;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Models.Extensions;

public static partial class IsShallowEqualExtensions
{
    private static readonly Lock _locker = new();

    private static readonly HashSet<Type> _comparersInProgress = new();

    private static readonly IDictionary<Type, Delegate> _comparers = new Dictionary<Type, Delegate>();

    private static readonly IDictionary<Type, LambdaExpression> _rawComparers =
        new Dictionary<Type, LambdaExpression>();

    public static bool IsShallowEqual<T, TD>(this T value, TD data)
    {
        var mapper = Mapper.GetFor(Assembly.GetCallingAssembly());

        return value.IsShallowEqual(data, mapper);
    }

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
