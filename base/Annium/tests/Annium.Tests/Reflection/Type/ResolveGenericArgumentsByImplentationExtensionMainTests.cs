using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class ResolveGenericArgumentsByImplementationExtensionMainTests
{
    [Fact]
    public void TypeNull_Throws()
    {
        // assert
        Wrap.It(() => (null as System.Type)!.ResolveGenericArgumentsByImplementation(typeof(bool)))
            .Throws<ArgumentNullException>();
    }

    [Fact]
    public void TargetNull_Throws()
    {
        // assert
        Wrap.It(() => typeof(bool).ResolveGenericArgumentsByImplementation(null!))
            .Throws<ArgumentNullException>();
    }

    [Fact]
    public void BuildArgs_InferByDefinitions()
    {
        // assert
        typeof(ConstrainedComplex<,,,>).ResolveGenericArgumentsByImplementation(
                typeof(IGeneric<IGeneric<bool, IGeneric<bool, int>>>))!
            .IsEqual(new[]
            {
                typeof(IGeneric<bool, IGeneric<bool, int>>), typeof(bool), typeof(IGeneric<bool, int>), typeof(int)
            });
    }

    private class ConstrainedComplex<T1, T2, T3, T4> : IGeneric<T1> where T1 : IGeneric<T2, T3> where T3 : IGeneric<T2, T4>
    {
    }

    private interface IGeneric<T>
    {
    }

    private interface IGeneric<T1, T2>
    {
    }
}