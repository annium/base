using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

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
