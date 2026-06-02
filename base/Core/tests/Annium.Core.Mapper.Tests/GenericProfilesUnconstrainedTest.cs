using System;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests that an unconstrained generic profile is rejected by the mapper at resolution time.
/// </summary>
public class GenericProfilesUnconstrainedTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericProfilesUnconstrainedTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public GenericProfilesUnconstrainedTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(typeof(InvalidProfile<>)));
    }

    /// <summary>
    /// Tests that generic profiles fail appropriately when type constraints are violated.
    /// </summary>
    [Fact]
    public void GenericProfiles_Unconstrained_Fails()
    {
        // ResolveProfiles fires when IMapper is first resolved (lazy IEnumerable<Profile> dependency),
        // so the unconstrained profile must surface as an ArgumentException at Get<IMapper>() time.
        Wrap.It(() => Get<IMapper>()).Throws<ArgumentException>();
    }

    /// <summary>
    /// Invalid generic profile that attempts to map any type to D without constraints.
    /// </summary>
    private class InvalidProfile<T> : Profile
    {
        public InvalidProfile()
        {
            Map<T, D>(_ => new D());
        }
    }

    /// <summary>Target class with a lowercase name property.</summary>
    private class D
    {
        /// <summary>Gets or sets the lower-cased name produced by the mapping profile.</summary>
        public string LowerName { get; set; } = string.Empty;
    }
}
