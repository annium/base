using Annium.Core.DependencyInjection;
using Annium.Core.Mapper.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests that a generic profile with a type constraint is closed over every
/// auto-mapped subclass and produces working maps.
/// </summary>
public class GenericProfilesTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericProfilesTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public GenericProfilesTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(typeof(ValidProfile<>)));
    }

    /// <summary>
    /// Tests that generic profiles work correctly with constrained types.
    /// </summary>
    [Fact]
    public void GenericProfiles_Work()
    {
        // arrange
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

    /// <summary>Valid generic profile that maps types derived from A to D.</summary>
    private class ValidProfile<T> : Profile
        where T : A
    {
        public ValidProfile()
        {
            Map<T, D>(x => new D { LowerName = x.Name.ToLowerInvariant() });
        }
    }

    /// <summary>Base class for source types with a Name property.</summary>
    private class A
    {
        /// <summary>Gets or sets the name value used as the mapping source.</summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>Auto-mapped class that extends A with an Age property.</summary>
    [AutoMapped]
    private class B : A
    {
        /// <summary>Gets or sets the age value carried by this source instance.</summary>
        public int Age { get; set; }
    }

    /// <summary>Auto-mapped class that extends A with an IsAlive property.</summary>
    [AutoMapped]
    private class C : A
    {
        /// <summary>Gets or sets the alive flag carried by this source instance.</summary>
        public bool IsAlive { get; set; }
    }

    /// <summary>Target class with a lowercase name property.</summary>
    private class D
    {
        /// <summary>Gets or sets the lower-cased name produced by the mapping profile.</summary>
        public string LowerName { get; set; } = string.Empty;
    }
}
