using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// G25: Verifies that calling <c>Map&lt;A,B&gt;()</c> twice on the SAME profile instance applies
/// last-wins semantics — the second <c>.With(...)</c> replaces the first registration because
/// <c>Profile.Map&lt;TS,TD&gt;()</c> always writes a fresh builder into the dictionary.
/// </summary>
public class SingleProfile_LastWins_Test : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleProfile_LastWins_Test"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public SingleProfile_LastWins_Test(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile<TwoRegistrationsProfile>());
    }

    /// <summary>
    /// Within a single profile, the second call to Map&lt;A,B&gt;().With(...) on the same (A,B) pair
    /// must replace the first because Profile._mapConfigurations uses indexer assignment (last-wins).
    /// The mapper must therefore return Tag = "second".
    /// </summary>
    [Fact]
    public void SingleProfile_SecondMapRegistration_Wins()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new ProfileSource();

        // act
        var result = mapper.Map<ProfileTarget>(value);

        // assert — second registration wins over first
        result.Tag.Is("second");
    }

    /// <summary>Source DTO for single-profile last-wins test.</summary>
    public class ProfileSource
    {
        // intentionally empty — Tag is hardcoded in the mapping lambdas
    }

    /// <summary>Target DTO carrying a tag string.</summary>
    public class ProfileTarget
    {
        /// <summary>Gets or sets the tag produced by the winning mapping.</summary>
        public string Tag { get; set; } = string.Empty;
    }

    /// <summary>
    /// Profile that registers <see cref="ProfileSource"/>→<see cref="ProfileTarget"/> twice.
    /// The second <c>Map&lt;ProfileSource,ProfileTarget&gt;().With(...)</c> call silently replaces
    /// the first because <c>Profile.Map&lt;TS,TD&gt;()</c> overwrites <c>_mapConfigurations[(TS,TD)]</c>.
    /// </summary>
    public class TwoRegistrationsProfile : Profile
    {
        /// <summary>Registers the same pair twice; second registration must win.</summary>
        public TwoRegistrationsProfile()
        {
            Map<ProfileSource, ProfileTarget>().With(_ => new ProfileTarget { Tag = "first" });
            Map<ProfileSource, ProfileTarget>().With(_ => new ProfileTarget { Tag = "second" });
        }
    }
}

/// <summary>
/// Verifies that when two profiles register a mapping for the same (src, tgt) pair,
/// the MapBuilder applies "first profile wins" semantics — the second profile's
/// configuration and MapWith are both skipped, with the first profile's mapping retained.
/// </summary>
public class DuplicateProfileTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateProfileTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public DuplicateProfileTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile<FirstProfile>().AddProfile<SecondProfile>());
    }

    /// <summary>
    /// The first-registered profile's mapping must be the one applied; the second profile's
    /// conflicting mapping must be silently skipped (logged as Trace, not applied).
    /// </summary>
    [Fact]
    public void DuplicatePair_FirstProfileWins()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Source { Value = "x" };

        // act
        var result = mapper.Map<Target>(value);

        // assert — FirstProfile prefixes with "first:"; SecondProfile would have prefixed with "second:"
        result.Tag.Is("first:x");
    }

    /// <summary>Source DTO carrying a single string value.</summary>
    public class Source
    {
        /// <summary>Gets or sets the source value.</summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>Target DTO carrying a single tag.</summary>
    public class Target
    {
        /// <summary>Gets or sets the tag value.</summary>
        public string Tag { get; set; } = string.Empty;
    }

    /// <summary>First-registered profile mapping <see cref="Source"/> to <see cref="Target"/>.</summary>
    public class FirstProfile : Profile
    {
        /// <summary>Initializes the first profile.</summary>
        public FirstProfile()
        {
            Map<Source, Target>(x => new Target { Tag = "first:" + x.Value });
        }
    }

    /// <summary>Conflicting second profile that should be skipped under first-wins semantics.</summary>
    public class SecondProfile : Profile
    {
        /// <summary>Initializes the second profile.</summary>
        public SecondProfile()
        {
            Map<Source, Target>(x => new Target { Tag = "second:" + x.Value });
        }
    }
}
