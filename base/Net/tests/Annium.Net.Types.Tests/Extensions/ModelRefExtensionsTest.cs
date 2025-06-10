using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Types.Tests.Extensions;

/// <summary>
/// Tests for model reference extension methods
/// </summary>
public class ModelRefExtensionsTest
{
    /// <summary>
    /// Tests IsFor method with struct type references
    /// </summary>
    [Fact]
    public void IsFor_StructType()
    {
        var taskTRef = new StructRef(
            typeof(System.Threading.Tasks.Task<>).Namespace!,
            typeof(System.Threading.Tasks.Task<>).PureName(),
            new GenericParameterRef("T")
        );
        taskTRef.IsFor(typeof(System.Threading.Tasks.Task<>)).IsTrue();
        taskTRef.IsFor(typeof(Task<>)).IsFalse();
        taskTRef.IsFor(typeof(Task)).IsFalse();
    }

    /// <summary>
    /// Tests IsFor method with interface type references
    /// </summary>
    [Fact]
    public void IsFor_InterfaceType()
    {
        var taskTRef = new InterfaceRef(
            typeof(System.Collections.Generic.IEnumerable<>).Namespace!,
            typeof(System.Collections.Generic.IEnumerable<>).PureName(),
            new GenericParameterRef("T")
        );
        taskTRef.IsFor(typeof(System.Collections.Generic.IEnumerable<>)).IsTrue();
        taskTRef.IsFor(typeof(IEnumerable<>)).IsFalse();
        taskTRef.IsFor(typeof(IEnumerable)).IsFalse();
    }

    /// <summary>
    /// Tests IsFor method with enum type references
    /// </summary>
    [Fact]
    public void IsFor_EnumType()
    {
        var taskTRef = new EnumRef(typeof(System.UriKind).Namespace!, nameof(System.UriKind));
        taskTRef.IsFor(typeof(System.UriKind)).IsTrue();
        taskTRef.IsFor(typeof(UriKind)).IsFalse();
    }

    /// <summary>
    /// Tests IsFor method with struct model references
    /// </summary>
    [Fact]
    public void IsFor_StructModel()
    {
        var taskTRef = new StructRef(
            typeof(System.Threading.Tasks.Task<>).Namespace!,
            typeof(System.Threading.Tasks.Task<>).PureName(),
            new GenericParameterRef("T")
        );
        taskTRef.IsFor(ModelFor(typeof(System.Threading.Tasks.Task<>))).IsTrue();
        taskTRef.IsFor(ModelFor(typeof(Task<>))).IsFalse();
        taskTRef.IsFor(ModelFor(typeof(Task))).IsFalse();
    }

    /// <summary>
    /// Tests IsFor method with interface model references
    /// </summary>
    [Fact]
    public void IsFor_InterfaceModel()
    {
        var taskTRef = new InterfaceRef(
            typeof(System.Collections.Generic.IEnumerable<>).Namespace!,
            typeof(System.Collections.Generic.IEnumerable<>).PureName(),
            new GenericParameterRef("T")
        );
        taskTRef.IsFor(ModelFor(typeof(System.Collections.Generic.IEnumerable<>))).IsTrue();
        taskTRef.IsFor(ModelFor(typeof(IEnumerable<>))).IsFalse();
        taskTRef.IsFor(ModelFor(typeof(IEnumerable))).IsFalse();
    }

    /// <summary>
    /// Tests IsFor method with enum model references
    /// </summary>
    [Fact]
    public void IsFor_EnumModel()
    {
        var taskTRef = new EnumRef(typeof(System.UriKind).Namespace!, nameof(System.UriKind));
        taskTRef.IsFor(ModelFor(typeof(System.UriKind))).IsTrue();
        taskTRef.IsFor(ModelFor(typeof(UriKind))).IsFalse();
    }

    /// <summary>
    /// Creates a model for the specified type
    /// </summary>
    /// <param name="type">The type to create a model for</param>
    /// <returns>The created model</returns>
    private static IModel ModelFor(Type type) =>
        type switch
        {
            { IsInterface: true } => BuildInterface(type),
            { IsEnum: true } => BuildEnum(type),
            _ => BuildStruct(type),
        };

    /// <summary>
    /// Builds a struct model for the specified type
    /// </summary>
    /// <param name="type">The struct type</param>
    /// <returns>The struct model</returns>
    private static StructModel BuildStruct(Type type)
    {
        var model = new StructModel(type.Namespace!.ToNamespace(), type.IsAbstract, type.PureName());
        model.SetArgs(
            Enumerable
                .Range(0, type.IsGenericType ? type.GetGenericArguments().Length : 0)
                .Select(i => new GenericParameterRef($"T{i}"))
                .ToArray()
        );

        return model;
    }

    /// <summary>
    /// Builds an interface model for the specified type
    /// </summary>
    /// <param name="type">The interface type</param>
    /// <returns>The interface model</returns>
    private static InterfaceModel BuildInterface(Type type)
    {
        var model = new InterfaceModel(type.Namespace!.ToNamespace(), type.PureName());
        model.SetArgs(
            Enumerable
                .Range(0, type.IsGenericType ? type.GetGenericArguments().Length : 0)
                .Select(i => new GenericParameterRef($"T{i}"))
                .ToArray()
        );

        return model;
    }

    /// <summary>
    /// Builds an enum model for the specified type
    /// </summary>
    /// <param name="type">The enum type</param>
    /// <returns>The enum model</returns>
    private static EnumModel BuildEnum(Type type)
    {
        var model = new EnumModel(type.Namespace!.ToNamespace(), type.PureName(), new Dictionary<string, long>());

        return model;
    }
}

/// <summary>
/// Test Task struct for model reference testing
/// </summary>
/// <typeparam name="T">The generic type parameter</typeparam>
file record struct Task<T>;

/// <summary>
/// Test IEnumerable interface for model reference testing
/// </summary>
/// <typeparam name="T">The generic type parameter</typeparam>
file interface IEnumerable<T>;

/// <summary>
/// Test UriKind enum for model reference testing
/// </summary>
file enum UriKind;
