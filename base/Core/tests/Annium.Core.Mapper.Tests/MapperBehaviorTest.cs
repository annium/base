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

/// <summary>
/// Tests the three null / instanceof branches of <see cref="IMapper.HasMap{T}"/> and
/// <see cref="IMapper.HasMap(object?,Type?)"/>.
/// </summary>
public class HasMapNullAndInstanceOfTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HasMapNullAndInstanceOfTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public HasMapNullAndInstanceOfTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// HasMap returns false when the source argument is null (generic overload).
    /// </summary>
    [Fact]
    public void HasMap_NullSource_ReturnsFalse()
    {
        var mapper = Get<IMapper>();

        mapper.HasMap<Base>(null).IsFalse();
    }

    /// <summary>
    /// HasMap returns false when the target type argument is null (non-generic overload).
    /// </summary>
    [Fact]
    public void HasMap_NullType_ReturnsFalse()
    {
        var mapper = Get<IMapper>();

        mapper.HasMap(new Derived(), null).IsFalse();
    }

    /// <summary>
    /// HasMap returns true when the source instance is already an instance of the target type
    /// (Derived is-a Base), exercising the <c>type.IsInstanceOfType(source)</c> short-circuit.
    /// </summary>
    [Fact]
    public void HasMap_DerivedIsInstanceOfBase_ReturnsTrue()
    {
        var mapper = Get<IMapper>();

        mapper.HasMap<Base>(new Derived()).IsTrue();
    }

    /// <summary>Base class.</summary>
    private class Base;

    /// <summary>Derived class — is-a <see cref="Base"/> by inheritance.</summary>
    private class Derived : Base;
}

/// <summary>
/// Verifies that calling <c>With()</c> twice on the same <c>IMapConfigurationBuilder</c>
/// throws <see cref="InvalidOperationException"/> immediately (at profile construction time).
/// </summary>
public class MapConfigurationBuilder_DoubleWith_ThrowsTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapConfigurationBuilder_DoubleWith_ThrowsTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public MapConfigurationBuilder_DoubleWith_ThrowsTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Constructing a profile that chains two <c>With()</c> calls for the same type pair
    /// throws <see cref="InvalidOperationException"/> immediately at profile construction time.
    /// </summary>
    [Fact]
    public void With_CalledTwiceOnSamePair_ThrowsInvalidOperationException()
    {
        // act + assert — the second With() call inside the DoubleWithProfile ctor throws instantly
        Wrap.It(() => new DoubleWithProfile()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Profile that intentionally calls <c>With()</c> twice for the same (A, B) type pair
    /// to exercise the double-call guard in <c>MapConfigurationBuilder.With()</c>.
    /// </summary>
    private class DoubleWithProfile : Profile
    {
        /// <summary>Initializes the profile — second With() call triggers the guard.</summary>
        public DoubleWithProfile()
        {
            Map<A, B>().With(x => new B { Value = x.Value }).With(x => new B { Value = x.Value + 1 });
        }
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
/// Verifies the documented HasMap invariant: HasMap returns FALSE for a type pair that
/// a resolver CAN build (e.g. AssignmentMapResolver covers any class with a default ctor
/// and matching properties) but for which NO profile has registered an explicit configuration.
/// HasMap probes HasConfiguration, not HasMapping, so resolver-buildable pairs are NOT counted.
/// </summary>
public class HasMap_ResolverBuildableButNoConfiguration_Test : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HasMap_ResolverBuildableButNoConfiguration_Test"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public HasMap_ResolverBuildableButNoConfiguration_Test(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        // No AddProfile — Source→Target has NO registered configuration.
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// HasMap returns false for Source→Target even though AssignmentMapResolver can build
    /// the mapping (matching Name property + default ctor), because no profile registered a configuration.
    /// </summary>
    [Fact]
    public void HasMap_NoProfleConfig_ReturnsFalseEvenWhenBuildable()
    {
        var mapper = Get<IMapper>();

        // HasMap must be false — no configuration was registered
        mapper.HasMap<Target>(new Source()).IsFalse();
    }

    /// <summary>
    /// Confirms the pair is genuinely buildable: Map succeeds despite HasMap returning false,
    /// proving HasMap probes configuration only (not resolver capability).
    /// </summary>
    [Fact]
    public void Map_NoProfleConfig_StillSucceedsProving_Buildability()
    {
        var mapper = Get<IMapper>();
        var source = new Source { Name = "test" };

        // Map must succeed — AssignmentMapResolver can handle this pair
        var result = mapper.Map<Target>(source);
        result.Name.Is(source.Name);
    }

    /// <summary>Source type with a single property.</summary>
    private class Source
    {
        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Target type with a default constructor and a matching property.</summary>
    private class Target
    {
        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }
}
