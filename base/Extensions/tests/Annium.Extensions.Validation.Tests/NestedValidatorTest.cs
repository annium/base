using System.Threading.Tasks;
using Annium.Extensions.Validation.RuleExtensions;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.Extensions.Validation.Tests;

/// <summary>
/// Tests for nested validator inheritance scenarios.
/// Verifies that validators can inherit from other validators and combine validation rules.
/// </summary>
public class NestedValidatorTest : TestBase
{
    /// <summary>
    /// Tests that inherited validators work correctly by combining validation rules
    /// from both the base validator and the derived validator.
    /// Validates that both X and Y properties are validated with their respective rules.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Validation_InheritedValidatorWorks()
    {
        // arrange
        var validator = GetValidator<Foo>();

        // act
        var result = await validator.ValidateAsync(new Foo());

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(2);
        result.LabeledErrors.At(nameof(Foo.X)).At(0).Is("Value is less, than given minimum");
        result.LabeledErrors.At(nameof(Foo.Y)).At(0).Is("Value is less, than given minimum");
    }

    /// <summary>
    /// Validator for Foo that inherits from BarValidator and adds additional validation rules.
    /// Demonstrates validator inheritance where derived validators can extend base validation logic.
    /// </summary>
    public class FooValidator : BarValidator<Foo>
    {
        /// <summary>
        /// Initializes a new instance of the FooValidator class.
        /// Adds validation rule for the Y property requiring it to be greater than 1.
        /// </summary>
        public FooValidator()
        {
            Field(x => x.Y).GreaterThan(1);
        }
    }

    /// <summary>
    /// Generic base validator for types that inherit from Bar.
    /// Provides common validation rules that can be inherited by derived validators.
    /// </summary>
    /// <typeparam name="T">The type being validated, must inherit from Bar</typeparam>
    public class BarValidator<T> : Validator<T>
        where T : Bar
    {
        /// <summary>
        /// Initializes a new instance of the BarValidator class.
        /// Adds validation rule for the X property requiring it to be greater than 1.
        /// </summary>
        public BarValidator()
        {
            Field(x => x.X).GreaterThan(1);
        }
    }

    /// <summary>
    /// Test data class that extends Bar with an additional property.
    /// Used to test inheritance scenarios in validation.
    /// </summary>
    public class Foo : Bar
    {
        /// <summary>
        /// Gets or sets the Y value used for testing additional validation rules in derived classes.
        /// </summary>
        public int Y { get; set; }
    }

    /// <summary>
    /// Base test data class with a simple integer property.
    /// Used as a base class to test validator inheritance scenarios.
    /// </summary>
    public class Bar
    {
        /// <summary>
        /// Gets or sets the X value used for testing base validation rules.
        /// </summary>
        public int X { get; set; }
    }
}
