using System;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Mapper;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Models.Extensions;

public static partial class IsShallowEqualExtensions
{
    private static LambdaExpression BuildExtensionCallComparer(Type type, IMapper mapper)
    {
        var a = Expression.Parameter(type);
        var b = Expression.Parameter(type);
        var m = Expression.Constant(mapper);

        var method = typeof(IsShallowEqualExtensions).GetMethods()
            .Single(x => x.GetParameters().Length == 3)
            .MakeGenericMethod(type, type);

        return Expression.Lambda(Expression.Call(null, method, a, b, m), a, b);
    }
}