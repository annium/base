using System;
using System.Collections.Generic;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the IsCuriouslyRecurringGenericParameter extension method.
/// </summary>
public class IsCuriouslyRecurringGenericParameterExtensionTests
{
    /// <summary>
    /// Verifies that IsCuriouslyRecurringGenericParameter throws when called on null.
    /// </summary>
    [Fact]
    public void IsCuriouslyRecurringGenericParameter_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.IsCuriouslyRecurringGenericParameter()).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that IsCuriouslyRecurringGenericParameter works correctly.
    /// </summary>
    [Fact]
    public void IsCuriouslyRecurringGenericParameter_Works()
    {
        // assert
        typeof(bool).IsCuriouslyRecurringGenericParameter().IsFalse();
        typeof(IEnumerable<>).GetGenericArguments()[0].IsCuriouslyRecurringGenericParameter().IsFalse();
        typeof(Demo<>).GetGenericArguments()[0].IsCuriouslyRecurringGenericParameter().IsTrue();
    }

    /// <summary>
    /// A demo class used for testing curiously recurring generic parameters.
    /// </summary>
    private class Demo<T>
        where T : Demo<T>;
}
