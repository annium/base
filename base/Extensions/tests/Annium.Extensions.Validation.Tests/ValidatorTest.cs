using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

// ReSharper disable UnusedType.Local

namespace Annium.Extensions.Validation.Tests;

/// <summary>
/// Tests for core validator functionality including labeling, null handling,
/// and interface-based validation composition.
/// </summary>
public class ValidatorTest : TestBase
{
    /// <summary>
    /// Tests that validation of null objects without labels produces plain errors.
    /// Verifies that null validation errors are reported as plain errors when no label is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Validation_NullWithoutLabel_UsesPlainError()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var result = await validator.ValidateAsync(null!);

        // assert
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("Value is null");
    }

    /// <summary>
    /// Tests that validation of null objects with labels produces labeled errors.
    /// Verifies that null validation errors use the provided label for error reporting.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Validation_NullWithLabel_UsesLabelForLabeledError()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var result = await validator.ValidateAsync(null!, "nested");

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At("nested").At(0).Is("Value is null");
    }

    /// <summary>
    /// Tests that validation without labels uses property names as labels.
    /// Verifies that validation errors automatically use property names when no explicit label is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Validation_WithoutLabel_UsesPropertyNameAsLabel()
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
    /// Tests that validation with labels combines the label and property name.
    /// Verifies that validation errors use both the provided label and property name in a hierarchical format.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Validation_WithLabel_UsesLabelAndPropertyNameAsLabel()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var result = await validator.ValidateAsync(new Person(), "nested");

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At($"nested.{nameof(Person.Name)}").At(0).Is("Value is required");
    }

    /// <summary>
    /// Tests that validation works correctly for objects implementing multiple interfaces.
    /// Verifies that validators for different interfaces are properly composed and applied.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Validation_CompoundThroughInterfaces()
    {
        // arrange
        var validator = GetValidator<User>();

        // act
        var result = await validator.ValidateAsync(new User());

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(2);
        result.LabeledErrors.At(nameof(User.Email)).At(0).Is("Value is required");
        result.LabeledErrors.At(nameof(User.Login)).At(0).Is("Value is required");
    }

    /// <summary>
    /// Test data class that implements multiple interfaces for testing compound validation.
    /// Used to verify that validators for different interfaces work together correctly.
    /// </summary>
    private class User : IEmail, ILogin
    {
        /// <summary>
        /// Gets or sets the email address for testing email validation rules.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the login name for testing login validation rules.
        /// </summary>
        public string Login { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interface defining email property for testing interface-based validation composition.
    /// </summary>
    private interface IEmail
    {
        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        string Email { get; set; }
    }

    /// <summary>
    /// Interface defining login property for testing interface-based validation composition.
    /// </summary>
    private interface ILogin
    {
        /// <summary>
        /// Gets or sets the login name.
        /// </summary>
        string Login { get; set; }
    }

    /// <summary>
    /// Validator for objects implementing IEmail interface.
    /// Tests interface-based validation composition.
    /// </summary>
    private class EmailValidator : Validator<IEmail>
    {
        /// <summary>
        /// Initializes a new instance of the EmailValidator class.
        /// Configures validation rules for the Email property.
        /// </summary>
        public EmailValidator()
        {
            Field(p => p.Email).Required();
        }
    }

    /// <summary>
    /// Validator for objects implementing ILogin interface.
    /// Tests interface-based validation composition.
    /// </summary>
    private class LoginValidator : Validator<ILogin>
    {
        /// <summary>
        /// Initializes a new instance of the LoginValidator class.
        /// Configures validation rules for the Login property.
        /// </summary>
        public LoginValidator()
        {
            Field(p => p.Login).Required();
        }
    }

    /// <summary>
    /// Simple test data class with a name property.
    /// Used for testing basic validation scenarios.
    /// </summary>
    private class Person
    {
        /// <summary>
        /// Gets or sets the person's name for testing string validation rules.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validator for Person class with basic required field validation.
    /// Used for testing fundamental validation scenarios.
    /// </summary>
    private class PersonValidator : Validator<Person>
    {
        /// <summary>
        /// Initializes a new instance of the PersonValidator class.
        /// Configures the Name property as required.
        /// </summary>
        public PersonValidator()
        {
            Field(p => p.Name).Required();
        }
    }
}
