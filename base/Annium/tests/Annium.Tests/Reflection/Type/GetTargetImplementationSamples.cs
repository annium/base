using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection.Tests.Types.Extensions.GetTargetImplementation;

public class ParentOther<T1, T2> : Base<T1[], T2, bool, IEnumerable<T1[]>>, IParentOther<T1, T2> where T2 : struct
{
}

public class ParentTwo<T1, T2> : ParentOne<T1, IReadOnlyList<T2>>, IParentTwo<T1, T2> where T1 : struct where T2 : IEnumerable
{
}

public class ParentOne<T1, T2> : Base<List<T2>, T1, int, IEnumerable<List<T2>>>, IParentOne<T1, T2> where T1 : struct
{
}

public class Base<T1, T2, T3, T4> : IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
{
}

public struct BaseStruct<T1, T2, T3, T4> : IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
{
}

public interface IParentOther<T1, T2> : IBase<T1[], T2, bool, IEnumerable<T1[]>> where T2 : struct
{
}

public interface IParentTwo<T1, T2> : IParentOne<T1, IReadOnlyList<T2>> where T1 : struct where T2 : IEnumerable
{
}

public interface IParentOne<T1, T2> : IBase<List<T2>, T1, int, IEnumerable<List<T2>>> where T1 : struct
{
}

public interface IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
{
}

public struct StructParamatered
{
    public int X { get; }

    public StructParamatered(int x)
    {
        X = x;
    }
}

public struct StructParamaterless
{
}

public struct StructEnumerable : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

public interface IClassConstraint<T> where T : class
{
}

public interface IStructConstraint<T> where T : struct
{
}

public interface INewConstraint<T> where T : new()
{
}

public interface IParameterConstraint<T> where T : IEnumerable
{
}