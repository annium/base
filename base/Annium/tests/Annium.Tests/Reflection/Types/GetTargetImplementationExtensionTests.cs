using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Annium.Reflection.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the GetTargetImplementation extension method.
/// </summary>
public partial class GetTargetImplementationExtensionTests
{
    /// <summary>
    /// Verifies that GetTargetImplementation throws when called on null.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.GetTargetImplementation(typeof(bool))).Throws<ArgumentNullException>();
        Wrap.It(() => typeof(bool).GetTargetImplementation(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation throws when called on an open type.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_OpenType_Throws()
    {
        // assert
        Wrap.It(() => typeof(List<>).GetTargetImplementation(typeof(bool))).Throws<ArgumentException>();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns the target when the type is assignable.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_Assignable_ReturnsTarget()
    {
        // assert
        typeof(IList<int>).GetTargetImplementation(typeof(IEnumerable<int>)).Is(typeof(IEnumerable<int>));
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the type is not assignable and the target is not generic.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_NonAssignableNonGenericTaget_ReturnsNull()
    {
        // assert
        typeof(IList<int>).GetTargetImplementation(typeof(IEnumerable<object>)).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the class does not implement the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfClass_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(Array).GetTargetImplementation(typeof(List<>)).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns the implementation when the class implements the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfClass_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(ParentOne<long, bool>)
            .GetTargetImplementation(typeof(Base<,,,>))
            .Is(typeof(Base<List<bool>, long, int, IEnumerable<List<bool>>>));
        typeof(ParentTwo<long, Array>)
            .GetTargetImplementation(typeof(ParentOne<,>).BaseType!)
            .Is(typeof(Base<List<IReadOnlyList<Array>>, long, int, IEnumerable<List<IReadOnlyList<Array>>>>));
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the class implements the target generic definition but the target has unresolved arguments.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfClass_ImplementingTargetGenericDefinition_MixedTarget_UnresolvedArg_ReturnsNull()
    {
        // assert
        typeof(ParentOther<int, int>).GetTargetImplementation(typeof(ParentOne<,>).BaseType!).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the class does not implement the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfInterface_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(Array).GetTargetImplementation(typeof(IList<>)).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns the implementation when the class implements the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfInterface_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(ParentOne<long, bool>)
            .GetTargetImplementation(typeof(IBase<,,,>))
            .Is(typeof(IBase<List<bool>, long, int, IEnumerable<List<bool>>>));
        typeof(ParentTwo<long, Array>)
            .GetTargetImplementation(typeof(IParentOne<,>).GetInterface("IBase`4")!)
            .Is(typeof(IBase<List<IReadOnlyList<Array>>, long, int, IEnumerable<List<IReadOnlyList<Array>>>>));
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the class implements the target generic definition but the target has unresolved arguments.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfInterface_ImplementingTargetGenericDefinition_MixedTarget_UnresolvedArg_ReturnsNull()
    {
        // assert
        typeof(ParentOther<int, int>)
            .GetTargetImplementation(typeof(IParentOne<,>).GetInterface("IBase`4")!)
            .IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the class does not meet the struct constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfParam_StructRequired_ReturnsNull()
    {
        // assert
        typeof(Array).GetTargetImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the class does not meet the default constructor constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfParam_DefaultConstructorRequired_ReturnsNull()
    {
        // assert
        typeof(FileInfo).GetTargetImplementation(typeof(INewConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the class does not meet the constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfParam_ConstraintFails_ReturnsNull()
    {
        // assert
        typeof(FileInfo).GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns the implementation when the class meets the constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_ClassOfParam_ConstraintSucceed_ReturnsImplementation()
    {
        // assert
        typeof(Array)
            .GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0])
            .Is(typeof(Array));
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the struct does not implement the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_StructOfStruct_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(long).GetTargetImplementation(typeof(ValueTuple<>)).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns the implementation when the struct implements the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_StructOfStruct_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(ValueTuple<long, bool>)
            .GetTargetImplementation(typeof(ValueTuple<,>))
            .Is(typeof(ValueTuple<long, bool>));
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the struct does not implement the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_StructOfInterface_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(ValueTuple).GetTargetImplementation(typeof(IList<>)).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns the implementation when the struct implements the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_StructOfInterface_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(BaseStruct<string, bool, int, IEnumerable<string>>)
            .GetTargetImplementation(typeof(IBase<,,,>))
            .Is(typeof(IBase<string, bool, int, IEnumerable<string>>));
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the struct does not meet the struct constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_StructOfParam_StructRequired_ReturnsNull()
    {
        // assert
        typeof(long).GetTargetImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the struct does not meet the default constructor constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_StructOfParam_DefaultConstructorRequired_ReturnsNull()
    {
        // assert
        typeof(StructParamatered)
            .GetTargetImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
        typeof(StructParamaterless)
            .GetTargetImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .Is(typeof(StructParamaterless));
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the struct is a nullable value type.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_StructOfParam_NullableValueType_ReturnsNull()
    {
        // assert
        typeof(bool?).GetTargetImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the struct does not meet the constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_StructOfParam_ConstraintFails_ReturnsNull()
    {
        // assert
        typeof(ValueTuple)
            .GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns the implementation when the struct meets the constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_StructOfParam_ConstraintSucceed_ReturnsImplementation()
    {
        // assert
        typeof(StructEnumerable)
            .GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0])
            .Is(typeof(StructEnumerable));
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the interface does not implement the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_InterfaceOfInterface_NotImplementingTargetGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(IEnumerable).GetTargetImplementation(typeof(IEnumerable<>)).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns the implementation when the interface implements the target generic definition.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_InterfaceOfInterface_ImplementingTargetGenericDefinition_GenericDefinitionTarget_ReturnsImplementation()
    {
        // assert
        typeof(IDictionary<long, bool>)
            .GetTargetImplementation(typeof(IDictionary<,>))
            .Is(typeof(IDictionary<long, bool>));
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the interface does not meet the class constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_InterfaceOfParam_ClassRequired_ReturnsNull()
    {
        // assert
        typeof(IEnumerable).GetTargetImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the interface does not meet the default constructor constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_InterfaceOfParam_DefaultConstructorRequired_ReturnsNull()
    {
        // assert
        typeof(IEnumerable).GetTargetImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0]).IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns null when the interface does not meet the constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_InterfaceOfParam_ConstraintFails_ReturnsNull()
    {
        // assert
        typeof(IDisposable)
            .GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    /// <summary>
    /// Verifies that GetTargetImplementation returns the implementation when the interface meets the constraint.
    /// </summary>
    [Fact]
    public void GetTargetImplementation_InterfaceOfParam_ConstraintSucceed_ReturnsImplementation()
    {
        // assert
        typeof(IEnumerable<int>)
            .GetTargetImplementation(typeof(IParameterConstraint<>).GetGenericArguments()[0])
            .Is(typeof(IEnumerable<int>));
    }

    /// <summary>
    /// A class used for testing parent other inheritance.
    /// </summary>
    private class ParentOther<T1, T2> : Base<T1[], T2, bool, IEnumerable<T1[]>>, IParentOther<T1, T2>
        where T2 : struct;

    /// <summary>
    /// A class used for testing parent two inheritance.
    /// </summary>
    private class ParentTwo<T1, T2> : ParentOne<T1, IReadOnlyList<T2>>, IParentTwo<T1, T2>
        where T1 : struct
        where T2 : IEnumerable;

    /// <summary>
    /// A class used for testing parent one inheritance.
    /// </summary>
    private class ParentOne<T1, T2> : Base<List<T2>, T1, int, IEnumerable<List<T2>>>, IParentOne<T1, T2>
        where T1 : struct;

    /// <summary>
    /// A base class used for testing inheritance.
    /// </summary>
    private class Base<T1, T2, T3, T4> : IBase<T1, T2, T3, T4>
        where T1 : class
        where T2 : struct
        where T4 : IEnumerable<T1>;

    /// <summary>
    /// A base struct used for testing inheritance.
    /// </summary>
    private struct BaseStruct<T1, T2, T3, T4> : IBase<T1, T2, T3, T4>
        where T1 : class
        where T2 : struct
        where T4 : IEnumerable<T1>;

    /// <summary>
    /// An interface used for testing parent other inheritance.
    /// </summary>
    private interface IParentOther<T1, T2> : IBase<T1[], T2, bool, IEnumerable<T1[]>>
        where T2 : struct;

    /// <summary>
    /// An interface used for testing parent two inheritance.
    /// </summary>
    private interface IParentTwo<T1, T2> : IParentOne<T1, IReadOnlyList<T2>>
        where T1 : struct
        where T2 : IEnumerable;

    /// <summary>
    /// An interface used for testing parent one inheritance.
    /// </summary>
    private interface IParentOne<T1, T2> : IBase<List<T2>, T1, int, IEnumerable<List<T2>>>
        where T1 : struct;

    /// <summary>
    /// An interface used for testing base inheritance.
    /// </summary>
    private interface IBase<T1, T2, T3, T4>
        where T1 : class
        where T2 : struct
        where T4 : IEnumerable<T1>;

    /// <summary>
    /// A struct used for testing parameterized inheritance.
    /// </summary>
    private readonly struct StructParamatered
    {
        /// <summary>
        /// Gets or sets the X value.
        /// </summary>
        public int X { get; }

        public StructParamatered(int x)
        {
            X = x;
        }
    }

    /// <summary>
    /// A struct used for testing parameterless inheritance.
    /// </summary>
    private struct StructParamaterless;

    /// <summary>
    /// A struct used for testing enumerable inheritance.
    /// </summary>
    private struct StructEnumerable : IEnumerable
    {
        /// <summary>
        /// Gets the enumerator for the struct.
        /// </summary>
        /// <returns>An enumerator for the struct.</returns>
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An interface used for testing class constraints.
    /// </summary>
    private interface IClassConstraint<T>
        where T : class;

    /// <summary>
    /// An interface used for testing struct constraints.
    /// </summary>
    private interface IStructConstraint<T>
        where T : struct;

    /// <summary>
    /// An interface used for testing new constraints.
    /// </summary>
    private interface INewConstraint<T>
        where T : new();

    /// <summary>
    /// An interface used for testing parameter constraints.
    /// </summary>
    private interface IParameterConstraint<T>
        where T : IEnumerable;
}
