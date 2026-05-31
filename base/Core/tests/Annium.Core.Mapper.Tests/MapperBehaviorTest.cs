using System;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests that HasMap returns true for a pair explicitly configured in a profile.
/// </summary>
public class HasMapConfiguredTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HasMapConfiguredTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public HasMapConfiguredTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(p => p.Map<A, B>(x => new B { Value = x.Value })));
    }

    /// <summary>
    /// Tests that HasMap returns true for the profile-configured (A, B) pair.
    /// </summary>
    [Fact]
    public void HasMap_ConfiguredPair_ReturnsTrue()
    {
        var mapper = Get<IMapper>();

        mapper.HasMap<B>(new A()).IsTrue();
    }

    /// <summary>Source type.</summary>
    private class A
    {
        /// <summary>Gets or sets the value.</summary>
        public int Value { get; set; }
    }

    /// <summary>Target type.</summary>
    private class B
    {
        /// <summary>Gets or sets the value.</summary>
        public int Value { get; set; }
    }
}

/// <summary>
/// Tests that an exception thrown inside a mapping surfaces unwrapped (not as TargetInvocationException).
/// </summary>
public class MappingThrowsTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingThrowsTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public MappingThrowsTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        // a throw-expression cannot live in an expression-tree lambda; route through a static method call
        // (a valid expression-tree node) so the compiled delegate throws at runtime
        Register(c => c.AddMapper(autoload: false).AddProfile(p => p.Map<A, B>(x => Boom(x))));
    }

    /// <summary>
    /// Tests that the user exception is surfaced directly, with TargetInvocationException unwrapped.
    /// </summary>
    [Fact]
    public void Map_MappingThrows_InnerExceptionSurfaced()
    {
        var mapper = Get<IMapper>();

        Wrap.It(() => mapper.Map<B>(new A())).Throws<InvalidOperationException>();
    }

    /// <summary>Always throws; used to make a mapping fail at runtime.</summary>
    /// <param name="source">Ignored source.</param>
    /// <returns>Never returns.</returns>
    private static B Boom(A source) => throw new InvalidOperationException("boom");

    /// <summary>Source type.</summary>
    private class A;

    /// <summary>Target type.</summary>
    private class B;
}

/// <summary>
/// Tests that mapping a pair no resolver can handle throws <see cref="MappingException"/>.
/// </summary>
public class NoResolverTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoResolverTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public NoResolverTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Tests that mapping a plain class to an enum (handled by no resolver) throws MappingException.
    /// </summary>
    [Fact]
    public void Map_NoResolver_ThrowsMappingException()
    {
        var mapper = Get<IMapper>();

        Wrap.It(() => mapper.Map<Color>(new A())).Throws<MappingException>();
    }

    /// <summary>Source type with no path to the target enum.</summary>
    private class A;

    /// <summary>Enum target no resolver handles from a class source.</summary>
    private enum Color
    {
        /// <summary>Red.</summary>
        Red,
    }
}
