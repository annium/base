using System;
using System.Threading.Tasks;
using Annium.Data.Models.Extensions.IsShallowEqual;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

// ReSharper disable UnusedType.Local

namespace Annium.Extensions.Composition.Tests;

/// <summary>
/// Test class for testing composition functionality
/// </summary>
public class ComposerTest : TestBase
{
    /// <summary>
    /// Tests that composition with null value and no label produces plain error
    /// </summary>
    /// <returns>Task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Composition_NullWithoutLabel_UsesPlainError()
    {
        // arrange
        var composer = GetComposer<Person>();

        // act
        var result = await composer.ComposeAsync(null!);

        // assert
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("Value is null");
    }

    /// <summary>
    /// Tests that composition with null value and label produces labeled error
    /// </summary>
    /// <returns>Task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Composition_NullWithLabel_UsesLabelForLabeledError()
    {
        // arrange
        var composer = GetComposer<Person>();

        // act
        var result = await composer.ComposeAsync(null!, "nested");

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At("nested").At(0).Is("Value is null");
    }

    /// <summary>
    /// Tests that composition without label uses property name as label
    /// </summary>
    /// <returns>Task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Composition_WithoutLabel_UsesPropertyNameAsLabel()
    {
        // arrange
        var data = new Person();
        var composer = GetComposer<Person>();

        // act
        var result = await composer.ComposeAsync(data);

        // assert
        result.IsOk.IsTrue();
        data.Name.Is(nameof(Person.Name));
    }

    /// <summary>
    /// Tests composition of compound objects through interface implementations
    /// </summary>
    /// <returns>Task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Composition_CompoundThroughInterfaces()
    {
        // arrange
        var data = new User();
        var composer = GetComposer<User>();

        // act
        var result = await composer.ComposeAsync(data);

        // assert
        result.IsOk.IsTrue();
        data.Email.Is(nameof(User.Email));
        data.Login.Is(nameof(User.Login));
    }

    /// <summary>
    /// Tests conditional composition implementation
    /// </summary>
    /// <returns>Task representing the asynchronous test operation</returns>
    [Fact]
    public async Task Composition_When_ImplementsConditional()
    {
        // arrange
        var personWithoutUser = new Person();
        var personWithUser = new Person { UserId = Guid.NewGuid() };
        var composer = GetComposer<Person>();

        // act
        var resultWithoutUser = await composer.ComposeAsync(personWithoutUser);
        var resultWithUser = await composer.ComposeAsync(personWithUser);

        // assert
        resultWithoutUser.IsOk.IsTrue();
        personWithoutUser.IsShallowEqual(new Person { Name = nameof(Person.Name) });
        resultWithUser.IsOk.IsTrue();
        personWithUser.IsShallowEqual(
            new Person
            {
                Name = nameof(Person.Name),
                UserId = personWithUser.UserId,
                User = new User { Email = nameof(User.Email), Login = nameof(User.Login) },
            }
        );
    }

    /// <summary>
    /// Test user class implementing email and login interfaces
    /// </summary>
    private class User : IEmail, ILogin
    {
        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the login name
        /// </summary>
        public string Login { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interface for objects that have an email property
    /// </summary>
    private interface IEmail
    {
        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        string Email { get; set; }
    }

    /// <summary>
    /// Interface for objects that have a login property
    /// </summary>
    private interface ILogin
    {
        /// <summary>
        /// Gets or sets the login name
        /// </summary>
        string Login { get; set; }
    }

    /// <summary>
    /// Composer for email interface implementations
    /// </summary>
    private class EmailComposer : Composer<IEmail>
    {
        public EmailComposer()
        {
            Field(p => p.Email).LoadWith(ctx => ctx.Label);
        }
    }

    /// <summary>
    /// Composer for login interface implementations
    /// </summary>
    private class LoginComposer : Composer<ILogin>
    {
        public LoginComposer()
        {
            Field(p => p.Login).LoadWith(ctx => ctx.Label);
        }
    }

    /// <summary>
    /// Test person class with name, user ID, and user properties
    /// </summary>
    private class Person
    {
        /// <summary>
        /// Gets or sets the person's name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets the associated user
        /// </summary>
        public User? User { get; set; }
    }

    /// <summary>
    /// Composer for person objects
    /// </summary>
    private class PersonComposer : Composer<Person>
    {
        public PersonComposer()
        {
            Field(p => p.Name).LoadWith(ctx => ctx.Label);
            Field(p => p.User)
                .When(p => p.Root.UserId.HasValue)
                .LoadWith(_ => new User { Email = nameof(User.Email), Login = nameof(User.Login) });
        }
    }

    /// <summary>
    /// Test class representing a bad configuration
    /// </summary>
    private class Bad
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
