using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Annium.Reflection;
using Annium.Reflection.Tests.Types.Extensions.GetTargetImplementation;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class GetTargetImplementationExtensionTests
{
    [Fact]
    public void GetTargetImplementation_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as System.Type)!.GetTargetImplementation(typeof(bool))).Throws<ArgumentNullException>();
        Wrap.It(() => typeof(bool).GetTargetImplementation(null!)).Throws<ArgumentNullException>();
    }

    [Fact]
    public void GetTargetImplementation_OpenType_Throws()
    {
        // assert
        Wrap.It(() => typeof(List<>).GetTargetImplementation(typeof(bool))).Throws<ArgumentException>();
    }

    [Fact]
    public void GetTargetImplementation_Assignable_ReturnsTarget()
    {
        // assert
        typeof(IList<int>).GetTargetImplementation(typeof(IEnumerable<int>)).Is(typeof(IEnumerable<int>));
    }

    [Fact]
    public void GetTargetImplementation_NonAssignableNonGenericTaget_ReturnsNull()
    {
        // assert
        typeof(IList<int>).GetTargetImplementation(typeof(IEnumerable<object>)).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_ClassOfClass_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(Array).GetTargetImplementation(typeof(List<>)).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_ClassOfClass_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(ParentOne<long, bool>).GetTargetImplementation(typeof(Base<,,,>))
            .Is(typeof(Base<List<bool>, long, int, IEnumerable<List<bool>>>));
        typeof(ParentTwo<long, Array>).GetTargetImplementation(typeof(ParentOne<,>).BaseType!)
            .Is(typeof(Base<List<IReadOnlyList<Array>>, long, int, IEnumerable<List<IReadOnlyList<Array>>>>));
    }

    [Fact]
    public void GetTargetImplementation_ClassOfClass_ImplementingTargetGenericDefinition_MixedTarget_UnresolvedArg_ReturnsNull()
    {
        // assert
        typeof(ParentOther<int, int>).GetTargetImplementation(typeof(ParentOne<,>).BaseType!).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_ClassOfInterface_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(Array).GetTargetImplementation(typeof(IList<>)).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_ClassOfInterface_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(ParentOne<long, bool>).GetTargetImplementation(typeof(IBase<,,,>))
            .Is(typeof(IBase<List<bool>, long, int, IEnumerable<List<bool>>>));
        typeof(ParentTwo<long, Array>).GetTargetImplementation(typeof(IParentOne<,>).GetInterface("IBase`4")!)
            .Is(typeof(IBase<List<IReadOnlyList<Array>>, long, int, IEnumerable<List<IReadOnlyList<Array>>>>));
    }

    [Fact]
    public void GetTargetImplementation_ClassOfInterface_ImplementingTargetGenericDefinition_MixedTarget_UnresolvedArg_ReturnsNull()
    {
        // assert
        typeof(ParentOther<int, int>).GetTargetImplementation(typeof(IParentOne<,>).GetInterface("IBase`4")!).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_ClassOfParam_StructRequired_ReturnsNull()
    {
        // assert
        typeof(Array).GetTargetImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_ClassOfParam_DefaultConstructorRequired_ReturnsNull()
    {
        // assert
        typeof(FileInfo).GetTargetImplementation(typeof(INewConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_ClassOfParam_ConstraintFails_ReturnsNull()
    {
        // assert
        typeof(FileInfo).GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_ClassOfParam_ConstraintSucceed_ReturnsImplementation()
    {
        // assert
        typeof(Array).GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0]).Is(typeof(Array));
    }

    [Fact]
    public void GetTargetImplementation_StructOfStruct_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(long).GetTargetImplementation(typeof(ValueTuple<>)).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_StructOfStruct_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(ValueTuple<long, bool>).GetTargetImplementation(typeof(ValueTuple<,>))
            .Is(typeof(ValueTuple<long, bool>));
    }

    [Fact]
    public void GetTargetImplementation_StructOfInterface_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(ValueTuple).GetTargetImplementation(typeof(IList<>)).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_StructOfInterface_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(BaseStruct<string, bool, int, IEnumerable<string>>).GetTargetImplementation(typeof(IBase<,,,>))
            .Is(typeof(IBase<string, bool, int, IEnumerable<string>>));
    }

    [Fact]
    public void GetTargetImplementation_StructOfParam_StructRequired_ReturnsNull()
    {
        // assert
        typeof(long).GetTargetImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_StructOfParam_DefaultConstructorRequired_ReturnsNull()
    {
        // assert
        typeof(StructParamatered).GetTargetImplementation(typeof(INewConstraint<>).GetGenericArguments()[0]).IsDefault();
        typeof(StructParamaterless).GetTargetImplementation(typeof(INewConstraint<>).GetGenericArguments()[0]).Is(typeof(StructParamaterless));
    }

    [Fact]
    public void GetTargetImplementation_StructOfParam_NullableValueType_ReturnsNull()
    {
        // assert
        typeof(bool?).GetTargetImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_StructOfParam_ConstraintFails_ReturnsNull()
    {
        // assert
        typeof(ValueTuple).GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_StructOfParam_ConstraintSucceed_ReturnsImplementation()
    {
        // assert
        typeof(StructEnumerable).GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0]).Is(typeof(StructEnumerable));
    }

    [Fact]
    public void GetTargetImplementation_InterfaceOfInterface_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(IEnumerable).GetTargetImplementation(typeof(IEnumerable<>)).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_InterfaceOfInterface_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(IDictionary<long, bool>).GetTargetImplementation(typeof(IDictionary<,>))
            .Is(typeof(IDictionary<long, bool>));
    }

    [Fact]
    public void GetTargetImplementation_InterfaceOfParam_ClassRequired_ReturnsNull()
    {
        // assert
        typeof(IEnumerable).GetTargetImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_InterfaceOfParam_DefaultConstructorRequired_ReturnsNull()
    {
        // assert
        typeof(IEnumerable).GetTargetImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_InterfaceOfParam_ConstraintFails_ReturnsNull()
    {
        // assert
        typeof(IDisposable).GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    [Fact]
    public void GetTargetImplementation_InterfaceOfParam_ConstraintSucceed_ReturnsImplementation()
    {
        // assert
        typeof(IEnumerable<int>).GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0]).Is(typeof(IEnumerable<int>));
    }
}