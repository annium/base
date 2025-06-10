using System;
using System.Collections.Generic;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

/// <summary>
/// Represents a struct or class type model with support for inheritance, generic arguments, and field definitions.
/// </summary>
/// <param name="Namespace">The namespace containing this struct</param>
/// <param name="IsAbstract">Whether this struct is abstract</param>
/// <param name="Name">The name of this struct</param>
public sealed record StructModel(Namespace Namespace, bool IsAbstract, string Name) : IGenericModel
{
    /// <summary>
    /// The generic type arguments for this struct.
    /// </summary>
    public IReadOnlyList<IRef> Args { get; private set; } = Array.Empty<IRef>();

    /// <summary>
    /// The base class that this struct inherits from, if any.
    /// </summary>
    public IRef? Base { get; private set; }

    /// <summary>
    /// The interfaces that this struct implements.
    /// </summary>
    public IReadOnlyList<IRef> Interfaces { get; private set; } = Array.Empty<IRef>();

    /// <summary>
    /// The field definitions (properties and fields) for this struct.
    /// </summary>
    public IReadOnlyList<FieldModel> Fields { get; private set; } = Array.Empty<FieldModel>();

    /// <summary>
    /// Sets the generic type arguments for this struct.
    /// </summary>
    /// <param name="args">The generic type arguments</param>
    public void SetArgs(IReadOnlyList<IRef> args)
    {
        Args = args;
    }

    /// <summary>
    /// Sets the base class for this struct.
    /// </summary>
    /// <param name="base">The base class reference</param>
    public void SetBase(IRef @base)
    {
        Base = @base;
    }

    /// <summary>
    /// Sets the interfaces that this struct implements.
    /// </summary>
    /// <param name="interfaces">The implemented interfaces</param>
    public void SetInterfaces(IReadOnlyList<IRef> interfaces)
    {
        Interfaces = interfaces;
    }

    /// <summary>
    /// Sets the field definitions for this struct.
    /// </summary>
    /// <param name="fields">The field definitions</param>
    public void SetFields(IReadOnlyList<FieldModel> fields)
    {
        Fields = fields;
    }

    /// <summary>
    /// Returns a string representation of this struct model.
    /// </summary>
    /// <returns>A string in the format "struct Namespace.Name"</returns>
    public override string ToString() => $"struct {Namespace}.{Name}";
}
