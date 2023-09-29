using System;
using System.Threading.Tasks;
using Annium.Data.Models.Extensions;
using Annium.Testing;
using Xunit;

// ReSharper disable UnusedType.Local

namespace Annium.Extensions.Composition.Tests;

public class ComposerTest : TestBase
{
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
        personWithoutUser.IsShallowEqual(new Person
        {
            Name = nameof(Person.Name),
        });
        resultWithUser.IsOk.IsTrue();
        personWithUser.IsShallowEqual(new Person
        {
            Name = nameof(Person.Name),
            UserId = personWithUser.UserId,
            User = new User
            {
                Email = nameof(User.Email),
                Login = nameof(User.Login),
            }
        });
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

    private class EmailComposer : Composer<IEmail>
    {
        public EmailComposer()
        {
            Field(p => p.Email).LoadWith(ctx => ctx.Label);
        }
    }

    private class LoginComposer : Composer<ILogin>
    {
        public LoginComposer()
        {
            Field(p => p.Login).LoadWith(ctx => ctx.Label);
        }
    }

    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public User? User { get; set; }
    }

    private class PersonComposer : Composer<Person>
    {
        public PersonComposer()
        {
            Field(p => p.Name).LoadWith(ctx => ctx.Label);
            Field(p => p.User).When(p => p.Root.UserId.HasValue).LoadWith(_ => new User { Email = nameof(User.Email), Login = nameof(User.Login) });
        }
    }

    private class Bad
    {
        public string Name { get; set; } = string.Empty;
    }
}