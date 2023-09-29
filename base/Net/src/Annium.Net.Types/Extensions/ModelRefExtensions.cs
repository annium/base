using System;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Extensions;

public static class ModelRefExtensions
{
    public static bool IsFor(this IModelRef @ref, Type type) => @ref switch
    {
        IGenericModelRef x => x.Namespace == type.Namespace &&
            x.Name == type.PureName() &&
            type.IsGenericType &&
            x.Args.Length == type.GetGenericArguments().Length,
        _ => @ref.Namespace == type.Namespace && @ref.Name == type.PureName()
    };

    public static bool IsFor(this IModelRef @ref, IModel model) => model switch
    {
        IGenericModel m => @ref switch
        {
            IGenericModelRef x => x.Namespace == m.Namespace.ToString() &&
                x.Name == m.Name &&
                x.Args.Length == m.Args.Count,
            _ => false
        },
        _ => @ref switch
        {
            IGenericModelRef => false,
            _                => @ref.Namespace == model.Namespace.ToString() && @ref.Name == model.Name
        }
    };
}