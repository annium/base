using System.Text.RegularExpressions;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Validation.Tests.Rules;

/// <summary>
/// Tests when the pattern given to the Matches rule is turned into a Regex. Doing it per validated value
/// recompiled the pattern every time and kept its syntax errors hidden until a request arrived.
/// </summary>
public class RegexRuleTest : TestBase
{
    /// <summary>
    /// A malformed pattern is reported when the validator is built, not on the first value it sees. The
    /// pattern used to be compiled inside the per-value delegate, which both hid the syntax error until a
    /// request arrived and recompiled the pattern for every value.
    /// </summary>
    [Fact]
    public void Matches_MalformedPattern_ThrowsWhenTheValidatorIsBuilt()
    {
        // act & assert
        Wrap.It(() => GetValidator<Pattern>()).Throws<RegexParseException>();
    }
}

/// <summary>
/// Model whose validator uses a pattern that is not valid.
/// </summary>
public class Pattern
{
    /// <summary>
    /// Gets or sets the value to match.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Validator carrying a malformed pattern.
/// </summary>
public class PatternValidator : Validator<Pattern>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PatternValidator"/> class.
    /// </summary>
    public PatternValidator()
    {
        Field(x => x.Value).Matches("[");
    }
}
