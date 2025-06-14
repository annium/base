using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Validation.Tests.Rules;

/// <summary>
/// Tests for custom validation rules including conditional validation (When),
/// short-circuit validation (Then), and custom asynchronous validation rules.
/// </summary>
public class CustomRulesTest : TestBase
{
    /// <summary>
    /// Tests that the When rule correctly implements conditional validation.
    /// Verifies that validation rules are only applied when the condition is met.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task WhenRule_ImplementsConditional()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Name = "Max" });
        var resultBad = await validator.ValidateAsync(new Person { Name = "Max", Age = 16 });

        // assert
        resultGood.IsOk.IsTrue();
        resultBad.HasErrors.IsTrue();
        resultBad.LabeledErrors.Has(1);
        resultBad.LabeledErrors.At(nameof(Person.Age)).At(0).Is("Value doesn't match condition");
    }

    /// <summary>
    /// Tests that the Then rule correctly implements short-circuit validation.
    /// Verifies that subsequent rules are only executed if previous rules pass.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task ThenRule_ImplementsShortCircuit()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var result = await validator.ValidateAsync(new Person());

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At(nameof(Person.Name)).At(0).Is("Value is required");
    }

    /// <summary>
    /// Tests that custom asynchronous validation rules work correctly.
    /// Verifies that custom rules can access validation context and generate custom error messages.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task CustomAsyncRule_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var result = await validator.ValidateAsync(new Person { Name = "ho" });

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At(nameof(Person.Name)).At(0).Is($"{nameof(Person.Name)} value is too short");
    }

    /// <summary>
    /// Test data class for testing custom validation rules.
    /// Contains properties for testing conditional validation and custom async rules.
    /// </summary>
    private class Person
    {
        /// <summary>
        /// Gets or sets the person's name for testing required and custom async validation.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the person's age for testing conditional validation rules.
        /// </summary>
        public uint? Age { get; set; }
    }

    /// <summary>
    /// Validator for Person class demonstrating custom validation rules.
    /// Shows usage of Then (short-circuit), When (conditional), and custom async validation.
    /// </summary>
    // ReSharper disable once UnusedType.Local
    private class PersonValidator : Validator<Person>
    {
        /// <summary>
        /// Initializes a new instance of the PersonValidator class.
        /// Configures custom validation rules including conditional validation,
        /// short-circuit validation, and custom asynchronous validation logic.
        /// </summary>
        public PersonValidator()
        {
            Field(p => p.Name)
                .Required()
                .Then()
                .Add(
                    async (context, value) =>
                    {
                        await Task.CompletedTask;
                        if (value.Length < 3)
                            context.Error("{0} value is too short", context.Field);
                    }
                );
            Field(p => p.Age).When(age => age.HasValue).Must(age => age!.Value > 18);
        }
    }
}
