using System;
using Annium.Core.Mapper.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests for generic profile-based mapping in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map between generic types
/// - Handle generic type constraints
/// - Preserve generic type information during mapping
/// - Map between different generic type implementations
/// </remarks>
public class GenericProfilesTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericProfilesTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public GenericProfilesTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that generic profiles work correctly with constrained types.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Generic profiles can be used with different types that satisfy constraints
    /// - Auto-mapped types are handled correctly
    /// - The mapping rules are applied consistently across different types
    /// - The result matches the expected transformation
    /// </remarks>
    [Fact]
    public void GenericProfiles_Work()
    {
        // arrange
        Register(c => c.AddMapper(autoload: false).AddProfile(typeof(ValidProfile<>)));
        var mapper = Get<IMapper>();
        var b = new B { Name = "Mike", Age = 5 };
        var c = new C { Name = "Donny", IsAlive = true };

        // act
        var one = mapper.Map<D>(b);
        var two = mapper.Map<D>(c);

        // assert
        one.LowerName.Is("mike");
        two.LowerName.Is("donny");
    }

    /// <summary>
    /// Tests that generic profiles fail appropriately when type constraints are violated.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - The mapper throws an ArgumentException when type constraints are violated
    /// - Unconstrained generic profiles are not allowed
    /// - The error is caught and handled appropriately
    /// </remarks>
    [Fact]
    public void GenericProfiles_Unconstrained_Fails()
    {
        // arrange
        Register(c => c.AddMapper(autoload: false).AddProfile(typeof(InvalidProfile<>)));

        // assert
        Wrap.It(() => Get<IMapper>()).Throws<ArgumentException>();
    }

    /// <summary>
    /// Valid generic profile that maps types derived from A to D.
    /// </summary>
    /// <typeparam name="T">The source type, must be derived from A.</typeparam>
    /// <remarks>
    /// Maps the source type to D by converting the Name property to lowercase.
    /// </remarks>
    private class ValidProfile<T> : Profile
        where T : A
    {
        /// <summary>
        /// Initializes a new instance of the ValidProfile class.
        /// </summary>
        public ValidProfile()
        {
            Map<T, D>(x => new D { LowerName = x.Name.ToLowerInvariant() });
        }
    }

    /// <summary>
    /// Invalid generic profile that attempts to map any type to D.
    /// </summary>
    /// <typeparam name="T">The source type, with no constraints.</typeparam>
    /// <remarks>
    /// This profile is invalid because it has no type constraints and attempts to map any type to D.
    /// </remarks>
    private class InvalidProfile<T> : Profile
    {
        /// <summary>
        /// Initializes a new instance of the InvalidProfile class.
        /// </summary>
        public InvalidProfile()
        {
            Map<T, D>(x => new D());
        }
    }

    /// <summary>
    /// Base class for source types with a Name property.
    /// </summary>
    private class A
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Auto-mapped class that extends A with an Age property.
    /// </summary>
    [AutoMapped]
    private class B : A
    {
        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        public int Age { get; set; }
    }

    /// <summary>
    /// Auto-mapped class that extends A with an IsAlive property.
    /// </summary>
    [AutoMapped]
    private class C : A
    {
        /// <summary>
        /// Gets or sets whether the instance is alive.
        /// </summary>
        public bool IsAlive { get; set; }
    }

    /// <summary>
    /// Target class with a lowercase name property.
    /// </summary>
    private class D
    {
        /// <summary>
        /// Gets or sets the lowercase name.
        /// </summary>
        public string LowerName { get; set; } = string.Empty;
    }
}
