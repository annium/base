using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

// ReSharper disable UnusedType.Local

namespace Annium.Extensions.Validation.Tests;

public class ValidatorTest : TestBase
{
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

    private class User : IEmail, ILogin
    {
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
    }

    private interface IEmail
    {
        string Email { get; set; }
    }

    private interface ILogin
    {
        string Login { get; set; }
    }

    private class EmailValidator : Validator<IEmail>
    {
        public EmailValidator()
        {
            Field(p => p.Email).Required();
        }
    }

    private class LoginValidator : Validator<ILogin>
    {
        public LoginValidator()
        {
            Field(p => p.Login).Required();
        }
    }

    private class Person
    {
        public string Name { get; set; } = string.Empty;
    }

    private class PersonValidator : Validator<Person>
    {
        public PersonValidator()
        {
            Field(p => p.Name).Required();
        }
    }
}