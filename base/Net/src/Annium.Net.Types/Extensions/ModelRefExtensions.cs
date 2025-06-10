using System;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Extensions;

/// <summary>
/// Extension methods for model reference types providing type matching functionality.
/// </summary>
public static class ModelRefExtensions
{
    /// <summary>
    /// Determines whether this model reference matches the specified .NET type.
    /// Compares namespace, name, and generic argument count for compatibility.
    /// </summary>
    /// <param name="ref">The model reference to check</param>
    /// <param name="type">The .NET type to match against</param>
    /// <returns>True if the reference matches the type, false otherwise</returns>
    public static bool IsFor(this IModelRef @ref, Type type) =>
        @ref switch
        {
            IGenericModelRef x => x.Namespace == type.Namespace
                && x.Name == type.PureName()
                && type.IsGenericType
                && x.Args.Length == type.GetGenericArguments().Length,
            _ => @ref.Namespace == type.Namespace && @ref.Name == type.PureName(),
        };

    /// <summary>
    /// Determines whether this model reference matches the specified type model.
    /// Compares namespace, name, and generic argument count for compatibility.
    /// </summary>
    /// <param name="ref">The model reference to check</param>
    /// <param name="model">The type model to match against</param>
    /// <returns>True if the reference matches the model, false otherwise</returns>
    public static bool IsFor(this IModelRef @ref, IModel model) =>
        model switch
        {
            IGenericModel m => @ref switch
            {
                IGenericModelRef x => x.Namespace == m.Namespace.ToString()
                    && x.Name == m.Name
                    && x.Args.Length == m.Args.Count,
                _ => false,
            },
            _ => @ref switch
            {
                IGenericModelRef => false,
                _ => @ref.Namespace == model.Namespace.ToString() && @ref.Name == model.Name,
            },
        };
}
