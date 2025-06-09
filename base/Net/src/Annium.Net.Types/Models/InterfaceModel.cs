using System;
using System.Collections.Generic;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

/// <summary>
/// Represents an interface type model with support for generic arguments, inherited interfaces, and field definitions.
/// </summary>
/// <param name="Namespace">The namespace containing this interface</param>
/// <param name="Name">The name of this interface</param>
public sealed record InterfaceModel(Namespace Namespace, string Name) : IGenericModel
{
    /// <summary>
    /// The generic type arguments for this interface.
    /// </summary>
    public IReadOnlyList<IRef> Args { get; private set; } = Array.Empty<IRef>();

    /// <summary>
    /// The interfaces that this interface inherits from.
    /// </summary>
    public IReadOnlyList<IRef> Interfaces { get; private set; } = Array.Empty<IRef>();

    /// <summary>
    /// The field definitions (properties and methods) for this interface.
    /// </summary>
    public IReadOnlyList<FieldModel> Fields { get; private set; } = Array.Empty<FieldModel>();

    /// <summary>
    /// Sets the generic type arguments for this interface.
    /// </summary>
    /// <param name="args">The generic type arguments</param>
    public void SetArgs(IReadOnlyList<IRef> args)
    {
        Args = args;
    }

    /// <summary>
    /// Sets the interfaces that this interface inherits from.
    /// </summary>
    /// <param name="interfaces">The inherited interfaces</param>
    public void SetInterfaces(IReadOnlyList<IRef> interfaces)
    {
        Interfaces = interfaces;
    }

    /// <summary>
    /// Sets the field definitions for this interface.
    /// </summary>
    /// <param name="fields">The field definitions</param>
    public void SetFields(IReadOnlyList<FieldModel> fields)
    {
        Fields = fields;
    }

    /// <summary>
    /// Returns a string representation of this interface model.
    /// </summary>
    /// <returns>A string in the format "interface Namespace.Name"</returns>
    public override string ToString() => $"interface {Namespace}.{Name}";
}
