using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Validation.Tests.Rules;

/// <summary>
/// Tests for built-in validation rules including required, equality, length, range,
/// regex, custom conditions, and specialized validators like email and enum.
/// </summary>
public class BaseRulesTest : TestBase
{
    /// <summary>
    /// Tests that the Required rule works correctly for string properties.
    /// Verifies that non-empty strings pass validation and whitespace-only strings fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Required_String_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Name = "some" });
        var resultBad = await validator.ValidateAsync(new Person { Name = " " });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Name)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Name)).At(0).Is("Value is required");
    }

    /// <summary>
    /// Tests that the Required rule works correctly for nullable properties.
    /// Verifies that null values and non-zero values pass, but zero values fail validation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Required_Nullable_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Nullable = null });
        var resultGood2 = await validator.ValidateAsync(new Person { Nullable = 2 });
        var resultBad = await validator.ValidateAsync(new Person { Nullable = 0 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Nullable)).IsFalse();
        resultGood2.LabeledErrors.ContainsKey(nameof(Person.Nullable)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Nullable)).At(0).Is("Value is required");
    }

    /// <summary>
    /// Tests that the Required rule works correctly for non-string value types.
    /// Verifies that non-default values pass validation and default values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Required_NotString_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Age = 1 });
        var resultBad = await validator.ValidateAsync(new Person());

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Age)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Age)).At(0).Is("Value is required");
    }

    /// <summary>
    /// Tests that the Equal rule works correctly with constant values.
    /// Verifies that matching values pass validation and non-matching values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task EqualValue_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Fixed = "fixed value" });
        var resultBad = await validator.ValidateAsync(new Person { Fixed = "other value" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Fixed)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Fixed)).At(0).Is("Value is not equal to given");
    }

    /// <summary>
    /// Tests that the In rule works correctly with a collection of allowed values.
    /// Verifies that values in the collection pass validation and values not in the collection fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task In_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { OneOf = "one" });
        var resultBad = await validator.ValidateAsync(new Person { OneOf = "three" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.OneOf)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.OneOf)).At(0).Is("Value is not in given");
    }

    /// <summary>
    /// Tests that the Equal rule works correctly with property accessors.
    /// Verifies that values equal to another property pass validation and unequal values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task EqualAccessor_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Name = "some", SameAsName = "some" });
        var resultBad = await validator.ValidateAsync(new Person { Name = "some", SameAsName = "other" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.SameAsName)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.SameAsName)).At(0).Is("Value is not equal to given");
    }

    /// <summary>
    /// Tests that the NotEqual rule works correctly with constant values.
    /// Verifies that non-matching values pass validation and matching values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task NotEqualValue_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { NotFixed = "other value" });
        var resultBad = await validator.ValidateAsync(new Person { NotFixed = "fixed value" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.NotFixed)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.NotFixed)).At(0).Is("Value is equal to given");
    }

    /// <summary>
    /// Tests that the NotIn rule works correctly with a collection of forbidden values.
    /// Verifies that values not in the collection pass validation and values in the collection fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task NotIn_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { NotOneOf = "three" });
        var resultBad = await validator.ValidateAsync(new Person { NotOneOf = "one" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.NotOneOf)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.NotOneOf)).At(0).Is("Value is in given");
    }

    /// <summary>
    /// Tests that the NotEqual rule works correctly with property accessors.
    /// Verifies that values not equal to another property pass validation and equal values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task NotEqualAccessor_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Name = "some", NotSameAsName = "other" });
        var resultBad = await validator.ValidateAsync(new Person { Name = "some", NotSameAsName = "some" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.NotSameAsName)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.NotSameAsName)).At(0).Is("Value is equal to given");
    }

    /// <summary>
    /// Tests that the Length rule works correctly with both minimum and maximum constraints.
    /// Verifies that strings within the length range pass validation and strings outside the range fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task MinMaxLength_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MinMaxLength = "other" });
        var resultBadMin = await validator.ValidateAsync(new Person { MinMaxLength = "x" });
        var resultBadMax = await validator.ValidateAsync(new Person { MinMaxLength = "otherx" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MinMaxLength)).IsFalse();
        resultBadMin.LabeledErrors.At(nameof(Person.MinMaxLength)).At(0).Is("Value length is less, than 2");
        resultBadMax.LabeledErrors.At(nameof(Person.MinMaxLength)).At(0).Is("Value length is greater, than 5");
    }

    /// <summary>
    /// Tests that the MinLength rule works correctly.
    /// Verifies that strings meeting the minimum length pass validation and shorter strings fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task MinLength_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MinLength = "other" });
        var resultBad = await validator.ValidateAsync(new Person { MinLength = "x" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MinLength)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MinLength)).At(0).Is("Value length is less, than 2");
    }

    /// <summary>
    /// Tests that the MaxLength rule works correctly.
    /// Verifies that strings within the maximum length pass validation and longer strings fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task MaxLength_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MaxLength = "other" });
        var resultBad = await validator.ValidateAsync(new Person { MaxLength = "otherx" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MaxLength)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MaxLength)).At(0).Is("Value length is greater, than 5");
    }

    /// <summary>
    /// Tests that the Between rule works correctly with numeric ranges.
    /// Verifies that values within the range pass validation and values outside the range fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Between_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Between = 2 });
        var resultBadMin = await validator.ValidateAsync(new Person { Between = 1 });
        var resultBadMax = await validator.ValidateAsync(new Person { Between = 4 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Between)).IsFalse();
        resultBadMin.LabeledErrors.At(nameof(Person.Between)).At(0).Is("Value is less, than given minimum");
        resultBadMax.LabeledErrors.At(nameof(Person.Between)).At(0).Is("Value is greater, than given maximum");
    }

    /// <summary>
    /// Tests that the LessThan rule works correctly.
    /// Verifies that values less than the threshold pass validation and greater values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task LessThan_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { LessThan = 2 });
        var resultBad = await validator.ValidateAsync(new Person { LessThan = 3 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.LessThan)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.LessThan)).At(0).Is("Value is greater, than given maximum");
    }

    /// <summary>
    /// Tests that the LessThanOrEqual rule works correctly.
    /// Verifies that values less than or equal to the threshold pass validation and greater values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task LessThanOrEqual_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { LessThanOrEqual = 3 });
        var resultBad = await validator.ValidateAsync(new Person { LessThanOrEqual = 4 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.LessThanOrEqual)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.LessThanOrEqual)).At(0).Is("Value is greater, than given maximum");
    }

    /// <summary>
    /// Tests that the GreaterThan rule works correctly.
    /// Verifies that values greater than the threshold pass validation and lesser values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task GreaterThan_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { GreaterThan = 4 });
        var resultBad = await validator.ValidateAsync(new Person { GreaterThan = 3 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.GreaterThan)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.GreaterThan)).At(0).Is("Value is less, than given minimum");
    }

    /// <summary>
    /// Tests that the GreaterThanOrEqual rule works correctly.
    /// Verifies that values greater than or equal to the threshold pass validation and lesser values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task GreaterThanOrEqual_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { GreaterThanOrEqual = 3 });
        var resultBad = await validator.ValidateAsync(new Person { GreaterThanOrEqual = 2 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.GreaterThanOrEqual)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.GreaterThanOrEqual)).At(0).Is("Value is less, than given minimum");
    }

    /// <summary>
    /// Tests that the Matches rule works correctly with string regex patterns.
    /// Verifies that strings matching the pattern pass validation and non-matching strings fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task RegexString_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { RegexString = "yes" });
        var resultBad = await validator.ValidateAsync(new Person { RegexString = "yoda" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.RegexString)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.RegexString)).At(0).Is("Value doesn't match specified regex");
    }

    /// <summary>
    /// Tests that the Matches rule works correctly with compiled Regex instances.
    /// Verifies that strings matching the regex pass validation and non-matching strings fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task RegexInstance_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { RegexInstance = "yes" });
        var resultBad = await validator.ValidateAsync(new Person { RegexInstance = "yoda" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.RegexInstance)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.RegexInstance)).At(0).Is("Value doesn't match specified regex");
    }

    /// <summary>
    /// Tests that the Must rule works correctly with custom synchronous predicates.
    /// Verifies that values satisfying the predicate pass validation and others fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task MustField_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MustField = "x" });
        var resultBad = await validator.ValidateAsync(new Person { MustField = "y" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MustField)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MustField)).At(0).Is("Value doesn't match condition");
    }

    /// <summary>
    /// Tests that the Must rule works correctly with predicates that access the full object context.
    /// Verifies that values satisfying the context-aware predicate pass validation and others fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task MustValueField_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MustField = "a", MustValueField = "a" });
        var resultBad = await validator.ValidateAsync(new Person { MustField = "a", MustValueField = "b" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MustValueField)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MustValueField)).At(0).Is("Value doesn't match condition");
    }

    /// <summary>
    /// Tests that the Must rule works correctly with custom asynchronous predicates.
    /// Verifies that values satisfying the async predicate pass validation and others fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task MustFieldAsync_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MustFieldAsync = "x" });
        var resultBad = await validator.ValidateAsync(new Person { MustFieldAsync = "y" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MustFieldAsync)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MustFieldAsync)).At(0).Is("Value doesn't match condition");
    }

    /// <summary>
    /// Tests that the Must rule works correctly with asynchronous predicates that access the full object context.
    /// Verifies that values satisfying the context-aware async predicate pass validation and others fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task MustValueFieldAsync_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MustFieldAsync = "a", MustValueFieldAsync = "a" });
        var resultBad = await validator.ValidateAsync(new Person { MustFieldAsync = "a", MustValueFieldAsync = "b" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MustValueFieldAsync)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MustValueFieldAsync)).At(0).Is("Value doesn't match condition");
    }

    /// <summary>
    /// Tests that the Email rule works correctly for email address validation.
    /// Verifies that valid email formats pass validation and invalid formats fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Email_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Email = "yes@xx" });
        var resultBad = await validator.ValidateAsync(new Person { Email = "yoda@" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Email)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Email)).At(0).Is("Value is not an email");
    }

    /// <summary>
    /// Tests that the Enum rule works correctly for enum value validation.
    /// Verifies that valid enum values pass validation and invalid enum values fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Enum_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Enum = LogLevel.Debug | LogLevel.Info });
        var resultBad = await validator.ValidateAsync(new Person { Enum = (LogLevel)8 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Enum)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Enum)).At(0).Is("Value is not in expected range");
    }

    /// <summary>
    /// Comprehensive test data class with properties for testing all base validation rules.
    /// Contains properties for string, numeric, nullable, regex, email, enum, and custom validation scenarios.
    /// </summary>
    private class Person
    {
        /// <summary>
        /// Gets or sets the name for testing required string validation.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age for testing required non-string validation.
        /// </summary>
        public uint Age { get; set; }

        /// <summary>
        /// Gets or sets a nullable value for testing nullable required validation.
        /// </summary>
        public long? Nullable { get; set; }

        /// <summary>
        /// Gets or sets a value for testing equality to a fixed constant.
        /// </summary>
        public string Fixed { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value for testing inclusion in a collection.
        /// </summary>
        public string OneOf { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value for testing equality to another property.
        /// </summary>
        public string SameAsName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value for testing inequality to a fixed constant.
        /// </summary>
        public string NotFixed { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value for testing exclusion from a collection.
        /// </summary>
        public string NotOneOf { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value for testing inequality to another property.
        /// </summary>
        public string NotSameAsName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string for testing minimum and maximum length constraints.
        /// </summary>
        public string MinMaxLength { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string for testing minimum length constraints.
        /// </summary>
        public string MinLength { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string for testing maximum length constraints.
        /// </summary>
        public string MaxLength { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a numeric value for testing range constraints.
        /// </summary>
        public long Between { get; set; }

        /// <summary>
        /// Gets or sets a numeric value for testing less than constraints.
        /// </summary>
        public long LessThan { get; set; }

        /// <summary>
        /// Gets or sets a numeric value for testing less than or equal constraints.
        /// </summary>
        public long LessThanOrEqual { get; set; }

        /// <summary>
        /// Gets or sets a numeric value for testing greater than constraints.
        /// </summary>
        public long GreaterThan { get; set; }

        /// <summary>
        /// Gets or sets a numeric value for testing greater than or equal constraints.
        /// </summary>
        public long GreaterThanOrEqual { get; set; }

        /// <summary>
        /// Gets or sets a string for testing regex pattern matching with string patterns.
        /// </summary>
        public string RegexString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string for testing regex pattern matching with compiled Regex instances.
        /// </summary>
        public string RegexInstance { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string for testing custom synchronous Must conditions.
        /// </summary>
        public string MustField { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string for testing custom Must conditions with object context access.
        /// </summary>
        public string MustValueField { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string for testing custom asynchronous Must conditions.
        /// </summary>
        public string MustFieldAsync { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string for testing custom asynchronous Must conditions with object context access.
        /// </summary>
        public string MustValueFieldAsync { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an email address for testing email format validation.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an enum value for testing enum validation.
        /// </summary>
        public LogLevel Enum { get; set; }
    }

    /// <summary>
    /// Test enum with flags for testing enum validation rules.
    /// Contains various log levels that can be combined using bitwise operations.
    /// </summary>
    [Flags]
    private enum LogLevel
    {
        /// <summary>
        /// No logging level specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Trace level logging for detailed diagnostic information.
        /// </summary>
        Trace = 1,

        /// <summary>
        /// Debug level logging for debugging information.
        /// </summary>
        Debug = 2,

        /// <summary>
        /// Info level logging for general information.
        /// </summary>
        Info = 4,
    }

    /// <summary>
    /// Comprehensive validator for Person class demonstrating all built-in validation rules.
    /// Configures validation rules for string, numeric, regex, email, enum, and custom validation scenarios.
    /// </summary>
    // ReSharper disable once UnusedType.Local
    private class PersonValidator : Validator<Person>
    {
        /// <summary>
        /// Initializes a new instance of the PersonValidator class.
        /// Configures all available built-in validation rules for comprehensive testing.
        /// </summary>
        public PersonValidator()
        {
            Field(p => p.Name).Required();
            Field(p => p.Age).Required();
            Field(p => p.Nullable).Required();
            Field(p => p.Fixed).Equal("fixed value");
            Field(p => p.OneOf).In(new[] { "one", "two" });
            Field(p => p.SameAsName).Equal(p => p.Name);
            Field(p => p.NotFixed).NotEqual("fixed value");
            Field(p => p.NotOneOf).NotIn(new[] { "one", "two" });
            Field(p => p.NotSameAsName).NotEqual(p => p.Name);
            Field(p => p.MinMaxLength).Length(2, 5);
            Field(p => p.MinLength).MinLength(2);
            Field(p => p.MaxLength).MaxLength(5);
            Field(p => p.Between).Between(2, 3);
            Field(p => p.LessThan).LessThan(3);
            Field(p => p.LessThanOrEqual).LessThanOrEqual(3);
            Field(p => p.GreaterThan).GreaterThan(3);
            Field(p => p.GreaterThanOrEqual).GreaterThanOrEqual(3);
            Field(p => p.RegexString).Matches("^\\w{2,3}$");
            Field(p => p.RegexInstance).Matches(new Regex("^\\w{2,3}$"));
            Field(p => p.MustField).Must(x => x == "x");
            Field(p => p.MustValueField).Must((ctx, x) => x == ctx.MustField);
            Field(p => p.MustFieldAsync).Must(x => Task.FromResult(x == "x"));
            Field(p => p.MustValueFieldAsync).Must((ctx, x) => Task.FromResult(x == ctx.MustFieldAsync));
            Field(p => p.Email).Email();
            Field(p => p.Enum).Enum();
        }
    }
}
