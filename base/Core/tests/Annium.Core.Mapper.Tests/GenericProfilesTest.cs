using System;
using System.Threading.Tasks;
using Annium.Core.Mapper.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests for generic profile-based mapping in the mapper.
/// </summary>
public class GenericProfilesTest
{
    /// <summary>The xunit output helper used for capturing test log output.</summary>
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericProfilesTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public GenericProfilesTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    /// <summary>
    /// Tests that generic profiles work correctly with constrained types.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task GenericProfiles_Work()
    {
        // arrange
        await using var fx = new Fixture(_outputHelper);
        fx.Register(c => c.AddMapper(autoload: false).AddProfile(typeof(ValidProfile<>)));
        await fx.InitializeAsync();

        var mapper = fx.Get<IMapper>();
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
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task GenericProfiles_Unconstrained_Fails()
    {
        // arrange
        await using var fx = new Fixture(_outputHelper);
        fx.Register(c => c.AddMapper(autoload: false).AddProfile(typeof(InvalidProfile<>)));
        await fx.InitializeAsync();

        // assert — mapper resolution observes the unconstrained profile and throws.
        Wrap.It(() => fx.Get<IMapper>()).Throws<ArgumentException>();
    }

    /// <summary>DI + logging fixture for generic profile mapping tests, disposed asynchronously after each test.</summary>
    private sealed class Fixture(ITestOutputHelper outputHelper) : TestBase(outputHelper), IAsyncDisposable;

    /// <summary>
    /// Valid generic profile that maps types derived from A to D.
    /// </summary>
    private class ValidProfile<T> : Profile
        where T : A
    {
        public ValidProfile()
        {
            Map<T, D>(x => new D { LowerName = x.Name.ToLowerInvariant() });
        }
    }

    /// <summary>
    /// Invalid generic profile that attempts to map any type to D without constraints.
    /// </summary>
    private class InvalidProfile<T> : Profile
    {
        public InvalidProfile()
        {
            Map<T, D>(x => new D());
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
