using System;
using System.Threading.Tasks;
using Annium.Data.Models.Extensions;
using Annium.Testing;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailComposer"/> class.
        /// </summary>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginComposer"/> class.
        /// </summary>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonComposer"/> class.
        /// </summary>
        public PersonComposer()
        {
            Field(p => p.Name).LoadWith(ctx => ctx.Label);
            Field(p => p.User)
                .When(p => p.Root.UserId.HasValue)
                .LoadWith(_ => new User { Email = nameof(User.Email), Login = nameof(User.Login) });
        }
    }

    /// <summary>
    /// Two composers loading the same property is reported when the executor is built. They would race to
    /// set it, so whichever won would be arbitrary - and silent.
    /// </summary>
    [Fact]
    public void Compose_TwoComposersForOneProperty_Throws()
    {
        // act & assert
        var error = Wrap.It(() => GetComposer<Contested>()).Throws<InvalidOperationException>();
        error.Message.Contains(nameof(Contested.Value)).IsTrue("the message must name the contested property");
    }

    /// <summary>
    /// A type nobody wrote a composer for comes back composed successfully. The composer set is resolved by
    /// reflection, so "no rules registered" and "rules written but the assembly was never scanned" look the
    /// same from here — this pins which of the two the library reports.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Compose_TypeWithoutComposer_Succeeds()
    {
        // arrange
        var composer = GetComposer<Bad>();
        var value = new Bad { Name = "unchanged" };

        // act
        var result = await composer.ComposeAsync(value);

        // assert - nothing composed it, and that is reported as success
        result.IsOk.IsTrue("a type with no composer has nothing to compose");
        value.Name.Is("unchanged");
    }

    /// <summary>
    /// A type deliberately left without a composer, used to pin the no-composer path.
    /// </summary>
    private class Bad
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}

/// <summary>
/// Model whose single property two composers both claim.
/// </summary>
public class Contested
{
    /// <summary>
    /// Gets or sets the contested value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// One of the two composers claiming Contested.Value.
/// </summary>
public class ContestedComposerOne : Composer<Contested>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContestedComposerOne"/> class.
    /// </summary>
    public ContestedComposerOne()
    {
        Field(x => x.Value).LoadWith(_ => "one");
    }
}

/// <summary>
/// The other composer claiming Contested.Value.
/// </summary>
public class ContestedComposerTwo : Composer<Contested>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContestedComposerTwo"/> class.
    /// </summary>
    public ContestedComposerTwo()
    {
        Field(x => x.Value).LoadWith(_ => "two");
    }
}
