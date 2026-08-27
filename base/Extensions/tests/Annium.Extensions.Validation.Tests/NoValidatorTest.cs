using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Validation.Tests;

/// <summary>
/// Pins what validation does for a type nobody wrote rules for. A value with no registered validator comes
/// back valid — deliberately, since an unconstrained type has nothing to fail — which also means a validator
/// that exists but was never registered is indistinguishable from one that was never written.
/// </summary>
public class NoValidatorTest : TestBase
{
    /// <summary>
    /// A type with no rules passes rather than failing or throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_TypeWithoutRules_Passes()
    {
        // arrange
        var validator = GetValidator<Unconstrained>();

        // act
        var result = await validator.ValidateAsync(new Unconstrained { Name = string.Empty });

        // assert - nothing was checked, and that is reported as success
        result.IsOk.IsTrue("a type with no rules has nothing to fail");
        result.PlainErrors.IsEmpty();
    }

    /// <summary>
    /// Null is still rejected, even for a type with no rules: that check does not come from a validator.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_NullValue_Fails()
    {
        // arrange
        var validator = GetValidator<Unconstrained>();

        // act
        var result = await validator.ValidateAsync(null!);

        // assert
        result.HasErrors.IsTrue("null is rejected regardless of rules");
    }

    /// <summary>
    /// A type deliberately left without any validator.
    /// </summary>
    private class Unconstrained
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
