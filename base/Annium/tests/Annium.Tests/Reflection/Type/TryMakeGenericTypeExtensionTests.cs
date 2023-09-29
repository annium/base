using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class TryMakeGenericTypeExtensionTests
{
    [Fact]
    public void TryMakeGenericType_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as System.Type)!.TryMakeGenericType(out _)).Throws<ArgumentNullException>();
    }

    [Fact]
    public void ITryMakeGenericType_Works()
    {
        // assert
        typeof(Demo<>).TryMakeGenericType(out System.Type? result).IsFalse();
        result.IsDefault();

        typeof(Demo<>).TryMakeGenericType(out result, typeof(bool)).IsFalse();
        result.IsDefault();

        typeof(Demo<>).TryMakeGenericType(out result, typeof(object)).IsTrue();
        result.Is(typeof(Demo<object>));
    }

    private class Demo<T> where T : class
    {
    }
}