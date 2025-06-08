using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection.Tests.Types.Extensions.GetTargetImplementation;

/// <summary>
/// A class used for testing parent other inheritance.
/// </summary>
public class ParentOther<T1, T2> : Base<T1[], T2, bool, IEnumerable<T1[]>>, IParentOther<T1, T2>
    where T2 : struct;

/// <summary>
/// A class used for testing parent two inheritance.
/// </summary>
public class ParentTwo<T1, T2> : ParentOne<T1, IReadOnlyList<T2>>, IParentTwo<T1, T2>
    where T1 : struct
    where T2 : IEnumerable;

/// <summary>
/// A class used for testing parent one inheritance.
/// </summary>
public class ParentOne<T1, T2> : Base<List<T2>, T1, int, IEnumerable<List<T2>>>, IParentOne<T1, T2>
    where T1 : struct;

/// <summary>
/// A base class used for testing inheritance.
/// </summary>
public class Base<T1, T2, T3, T4> : IBase<T1, T2, T3, T4>
    where T1 : class
    where T2 : struct
    where T4 : IEnumerable<T1>;

/// <summary>
/// A base struct used for testing inheritance.
/// </summary>
public struct BaseStruct<T1, T2, T3, T4> : IBase<T1, T2, T3, T4>
    where T1 : class
    where T2 : struct
    where T4 : IEnumerable<T1>;

/// <summary>
/// An interface used for testing parent other inheritance.
/// </summary>
public interface IParentOther<T1, T2> : IBase<T1[], T2, bool, IEnumerable<T1[]>>
    where T2 : struct;

/// <summary>
/// An interface used for testing parent two inheritance.
/// </summary>
public interface IParentTwo<T1, T2> : IParentOne<T1, IReadOnlyList<T2>>
    where T1 : struct
    where T2 : IEnumerable;

/// <summary>
/// An interface used for testing parent one inheritance.
/// </summary>
public interface IParentOne<T1, T2> : IBase<List<T2>, T1, int, IEnumerable<List<T2>>>
    where T1 : struct;

/// <summary>
/// An interface used for testing base inheritance.
/// </summary>
public interface IBase<T1, T2, T3, T4>
    where T1 : class
    where T2 : struct
    where T4 : IEnumerable<T1>;

/// <summary>
/// A struct used for testing parameterized inheritance.
/// </summary>
public readonly struct StructParamatered
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
public struct StructParamaterless;

/// <summary>
/// A struct used for testing enumerable inheritance.
/// </summary>
public struct StructEnumerable : IEnumerable
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
public interface IClassConstraint<T>
    where T : class;

/// <summary>
/// An interface used for testing struct constraints.
/// </summary>
public interface IStructConstraint<T>
    where T : struct;

/// <summary>
/// An interface used for testing new constraints.
/// </summary>
public interface INewConstraint<T>
    where T : new();

/// <summary>
/// An interface used for testing parameter constraints.
/// </summary>
public interface IParameterConstraint<T>
    where T : IEnumerable;
