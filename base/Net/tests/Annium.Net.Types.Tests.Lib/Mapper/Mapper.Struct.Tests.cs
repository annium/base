using System;
using System.Collections.Generic;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.Testing.Collection;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for testing struct type mapping functionality
/// </summary>
public abstract class MapperStructTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperStructTestsBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected MapperStructTestsBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(testProvider, outputHelper) { }

    /// <summary>
    /// Tests mapping of empty struct types
    /// </summary>
    /// <param name="type">The struct type to test</param>
    protected void Empty_Base(Type type)
    {
        // arrange
        var target = type.ToContextualType();

        // act
        var modelRef = Map(target).As<StructRef>();

        // assert
        modelRef.Namespace.Is(type.Namespace);
        modelRef.Name.Is(type.FriendlyName());
        modelRef.Args.IsEmpty();
        Models.Has(1);
        var model = Models.At(0).As<StructModel>();
        model.Namespace.Is(type.GetNamespace());
        model.Name.Is(type.FriendlyName());
        model.Args.IsEmpty();
        model.Base.IsDefault();
        model.Interfaces.IsEmpty();
        model.Fields.IsEmpty();
    }

    /// <summary>
    /// Tests mapping of complex struct types with generic parameters, interfaces, and fields
    /// </summary>
    protected void Struct_Base()
    {
        // arrange
        var type = typeof(Struct<string, HashSet<string>>);
        var target = type.ToContextualType();

        // act
        var modelRef = Map(target).As<StructRef>();

        // assert
        modelRef.Namespace.Is(type.Namespace);
        modelRef.Name.Is(nameof(Struct<object, object>));
        modelRef.Args.Has(2);
        var refArg1 = modelRef.Args.At(0);
        refArg1.As<BaseTypeRef>().Name.Is(BaseType.String);
        var refArg2 = modelRef.Args.At(1);
        refArg2.As<ArrayRef>().Value.As<BaseTypeRef>().Name.Is(BaseType.String);

        Models.Has(5);
        var structModel = Models.At(0).As<StructModel>();
        structModel.Namespace.Is(typeof(Struct<,>).GetNamespace());
        structModel.Name.Is(nameof(Struct<object, object>));
        structModel.Args.Has(2);
        structModel.Args.At(0).Is(new GenericParameterRef("T1"));
        structModel.Args.At(1).Is(new GenericParameterRef("T2"));
        structModel.Base.IsDefault();
        structModel.Interfaces.Has(2);
        structModel
            .Interfaces.At(0)
            .Is(
                new InterfaceRef(
                    typeof(IMulti<,>).GetNamespace().ToString(),
                    nameof(IMulti<object, object>),
                    new GenericParameterRef("T2"),
                    new GenericParameterRef("T1")
                )
            );
        structModel
            .Interfaces.At(1)
            .Is(
                new InterfaceRef(
                    typeof(Struct<,>).GetNamespace().ToString(),
                    nameof(IUno<object>),
                    new StructRef(
                        typeof(Struct<,>).GetNamespace().ToString(),
                        nameof(Struct<object, object>),
                        new GenericParameterRef("T1"),
                        new GenericParameterRef("T2")
                    )
                )
            );
        structModel.Fields.Has(8);
        structModel.Fields.At(0).Is(new FieldModel(new BaseTypeRef(BaseType.String), nameof(Struct<int, int>.Name)));
        structModel
            .Fields.At(1)
            .Is(new FieldModel(new NullableRef(new BaseTypeRef(BaseType.String)), nameof(Struct<int, int>.Comment)));
        structModel
            .Fields.At(2)
            .Is(new FieldModel(new ArrayRef(new GenericParameterRef("T2")), nameof(Struct<int, int>.Data)));
        structModel
            .Fields.At(3)
            .Is(
                new FieldModel(
                    new RecordRef(new BaseTypeRef(BaseType.String), new GenericParameterRef("T1")),
                    nameof(Struct<int, int>.Values)
                )
            );
        structModel
            .Fields.At(4)
            .Is(new FieldModel(new NullableRef(new BaseTypeRef(BaseType.Int)), nameof(Struct<int, int>.Ttl)));
        structModel
            .Fields.At(5)
            .Is(
                new FieldModel(
                    new ArrayRef(
                        new StructRef(
                            typeof(Struct<,>).GetNamespace().ToString(),
                            nameof(Struct<object, object>),
                            new GenericParameterRef("T1"),
                            new GenericParameterRef("T2")
                        )
                    ),
                    nameof(Struct<int, int>.Items)
                )
            );
        structModel
            .Fields.At(6)
            .Is(
                new FieldModel(
                    new NullableRef(new StructRef(typeof(EmptyStruct).GetNamespace().ToString(), nameof(EmptyStruct))),
                    nameof(Struct<int, int>.Option)
                )
            );
        structModel
            .Fields.At(7)
            .Is(
                new FieldModel(
                    new ArrayRef(new StructRef(typeof(EmptyRecord).GetNamespace().ToString(), nameof(EmptyRecord))),
                    nameof(Struct<int, int>.Records)
                )
            );

        var multi = Models.At(1).As<InterfaceModel>();
        multi.Namespace.Is(typeof(IMulti<,>).GetNamespace());
        multi.Name.Is(nameof(IMulti<object, object>));
        multi.Args.Has(2);
        multi.Args.At(0).Is(new GenericParameterRef("T1"));
        multi.Args.At(1).Is(new GenericParameterRef("T2"));
        multi.Interfaces.IsEmpty();
        multi.Fields.Has(4);
        multi.Fields.At(0).Is(new FieldModel(new BaseTypeRef(BaseType.String), nameof(IMulti<int, int>.Name)));
        multi
            .Fields.At(1)
            .Is(new FieldModel(new NullableRef(new BaseTypeRef(BaseType.String)), nameof(IMulti<int, int>.Comment)));
        multi
            .Fields.At(2)
            .Is(new FieldModel(new ArrayRef(new GenericParameterRef("T1")), nameof(IMulti<int, int>.Data)));
        multi
            .Fields.At(3)
            .Is(
                new FieldModel(
                    new RecordRef(new BaseTypeRef(BaseType.String), new GenericParameterRef("T2")),
                    nameof(IMulti<int, int>.Values)
                )
            );

        var uno = Models.At(2).As<InterfaceModel>();
        uno.Namespace.Is(typeof(IUno<>).GetNamespace());
        uno.Name.Is(nameof(IUno<int>));
        uno.Args.Has(1);
        uno.Args.At(0).Is(new GenericParameterRef("T"));
        uno.Interfaces.IsEmpty();
        uno.Fields.Has(2);
        uno.Fields.At(0).Is(new FieldModel(new NullableRef(new BaseTypeRef(BaseType.Int)), nameof(IUno<int>.Ttl)));
        uno.Fields.At(1).Is(new FieldModel(new ArrayRef(new GenericParameterRef("T")), nameof(IUno<int>.Items)));

        var emptyStruct = Models.At(3).As<StructModel>();
        emptyStruct.Namespace.Is(typeof(EmptyStruct).GetNamespace());
        emptyStruct.Name.Is(nameof(EmptyStruct));
        emptyStruct.Args.IsEmpty();
        emptyStruct.Interfaces.IsEmpty();
        emptyStruct.Fields.IsEmpty();

        var emptyRecord = Models.At(4).As<StructModel>();
        emptyRecord.Namespace.Is(typeof(EmptyRecord).GetNamespace());
        emptyRecord.Name.Is(nameof(EmptyRecord));
        emptyRecord.Args.IsEmpty();
        emptyRecord.Interfaces.IsEmpty();
        emptyRecord.Fields.IsEmpty();
    }
}

/// <summary>
/// Empty struct for testing struct mapping
/// </summary>
file struct EmptyStruct;

/// <summary>
/// Empty record for testing struct mapping
/// </summary>
file record EmptyRecord;

/// <summary>
/// Multi-generic interface for testing struct mapping
/// </summary>
/// <typeparam name="T1">First generic type parameter</typeparam>
/// <typeparam name="T2">Second generic type parameter</typeparam>
file interface IMulti<T1, T2>
    where T1 : notnull
    where T2 : notnull
{
    /// <summary>
    /// Gets the name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the optional comment
    /// </summary>
    string? Comment { get; }

    /// <summary>
    /// Gets the data array
    /// </summary>
    T1[] Data { get; }

    /// <summary>
    /// Gets the values dictionary
    /// </summary>
    IDictionary<string, T2?> Values { get; }
}

/// <summary>
/// Single-generic interface for testing struct mapping
/// </summary>
/// <typeparam name="T">The generic type parameter</typeparam>
file interface IUno<T>
    where T : notnull
{
    /// <summary>
    /// Gets the optional time-to-live value
    /// </summary>
    int? Ttl { get; }

    /// <summary>
    /// Gets the items list
    /// </summary>
    List<T> Items { get; }
}

/// <summary>
/// Complex generic struct for testing struct mapping
/// </summary>
/// <typeparam name="T1">First generic type parameter</typeparam>
/// <typeparam name="T2">Second generic type parameter</typeparam>
/// <param name="Name">The name</param>
/// <param name="Comment">The optional comment</param>
/// <param name="Data">The data array</param>
/// <param name="Values">The values dictionary</param>
/// <param name="Ttl">The optional time-to-live</param>
/// <param name="Items">The nested items list</param>
/// <param name="Option">The optional empty struct</param>
/// <param name="Records">The records enumerable</param>
file record struct Struct<T1, T2>(
    string Name,
    string? Comment,
    T2[] Data,
    IDictionary<string, T1?> Values,
    int? Ttl,
    List<Struct<T1, T2>> Items,
    EmptyStruct? Option,
    IEnumerable<EmptyRecord> Records
) : IMulti<T2, T1>, IUno<Struct<T1, T2>>
    where T1 : notnull
    where T2 : notnull;
