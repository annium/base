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

public class ModelRefExtensionsTest
{
    [Fact]
    public void IsFor_StructType()
    {
        var taskTRef = new StructRef(typeof(System.Threading.Tasks.Task<>).Namespace!, typeof(System.Threading.Tasks.Task<>).PureName(), new GenericParameterRef("T"));
        taskTRef.IsFor(typeof(System.Threading.Tasks.Task<>)).IsTrue();
        taskTRef.IsFor(typeof(Task<>)).IsFalse();
        taskTRef.IsFor(typeof(Task)).IsFalse();
    }

    [Fact]
    public void IsFor_InterfaceType()
    {
        var taskTRef = new InterfaceRef(typeof(System.Collections.Generic.IEnumerable<>).Namespace!, typeof(System.Collections.Generic.IEnumerable<>).PureName(), new GenericParameterRef("T"));
        taskTRef.IsFor(typeof(System.Collections.Generic.IEnumerable<>)).IsTrue();
        taskTRef.IsFor(typeof(IEnumerable<>)).IsFalse();
        taskTRef.IsFor(typeof(IEnumerable)).IsFalse();
    }

    [Fact]
    public void IsFor_EnumType()
    {
        var taskTRef = new EnumRef(typeof(System.UriKind).Namespace!, nameof(System.UriKind));
        taskTRef.IsFor(typeof(System.UriKind)).IsTrue();
        taskTRef.IsFor(typeof(UriKind)).IsFalse();
    }

    [Fact]
    public void IsFor_StructModel()
    {
        var taskTRef = new StructRef(typeof(System.Threading.Tasks.Task<>).Namespace!, typeof(System.Threading.Tasks.Task<>).PureName(), new GenericParameterRef("T"));
        taskTRef.IsFor(ModelFor(typeof(System.Threading.Tasks.Task<>))).IsTrue();
        taskTRef.IsFor(ModelFor(typeof(Task<>))).IsFalse();
        taskTRef.IsFor(ModelFor(typeof(Task))).IsFalse();
    }

    [Fact]
    public void IsFor_InterfaceModel()
    {
        var taskTRef = new InterfaceRef(typeof(System.Collections.Generic.IEnumerable<>).Namespace!, typeof(System.Collections.Generic.IEnumerable<>).PureName(), new GenericParameterRef("T"));
        taskTRef.IsFor(ModelFor(typeof(System.Collections.Generic.IEnumerable<>))).IsTrue();
        taskTRef.IsFor(ModelFor(typeof(IEnumerable<>))).IsFalse();
        taskTRef.IsFor(ModelFor(typeof(IEnumerable))).IsFalse();
    }

    [Fact]
    public void IsFor_EnumModel()
    {
        var taskTRef = new EnumRef(typeof(System.UriKind).Namespace!, nameof(System.UriKind));
        taskTRef.IsFor(ModelFor(typeof(System.UriKind))).IsTrue();
        taskTRef.IsFor(ModelFor(typeof(UriKind))).IsFalse();
    }

    private static IModel ModelFor(Type type) => type switch
    {
        { IsInterface: true } => BuildInterface(type),
        { IsEnum: true }      => BuildEnum(type),
        _                     => BuildStruct(type),
    };

    private static StructModel BuildStruct(Type type)
    {
        var model = new StructModel(type.Namespace!.ToNamespace(), type.IsAbstract, type.PureName());
        model.SetArgs(Enumerable.Range(0, type.IsGenericType ? type.GetGenericArguments().Length : 0).Select(i => new GenericParameterRef($"T{i}")).ToArray());

        return model;
    }

    private static InterfaceModel BuildInterface(Type type)
    {
        var model = new InterfaceModel(type.Namespace!.ToNamespace(), type.PureName());
        model.SetArgs(Enumerable.Range(0, type.IsGenericType ? type.GetGenericArguments().Length : 0).Select(i => new GenericParameterRef($"T{i}")).ToArray());

        return model;
    }

    private static EnumModel BuildEnum(Type type)
    {
        var model = new EnumModel(type.Namespace!.ToNamespace(), type.PureName(), new Dictionary<string, long>());

        return model;
    }
}

file record struct Task<T>;

file interface IEnumerable<T>
{
}

file enum UriKind
{
}