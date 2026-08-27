using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Validation.Tests.Rules;

/// <summary>
/// Tests the uniqueness rules. Both overloads ask the caller whether something already exists - the rule a
/// registration form leans on - and neither was exercised by anything.
/// </summary>
public class UniqueRuleTest : TestBase
{
    /// <summary>
    /// A value the caller says is taken is rejected, and the message names the field and the value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Unique_ValueTaken_Fails()
    {
        // arrange
        var validator = GetValidator<Registration>();

        // act - the fixture treats this address as already registered
        var result = await validator.ValidateAsync(new Registration { Email = "taken@example.com" });

        // assert
        var error = result.LabeledErrors.At(nameof(Registration.Email)).At(0);
        error.Contains(nameof(Registration.Email)).IsTrue("the message must name the field");
        error.Contains("taken@example.com").IsTrue("and the value that is already in use");
    }

    /// <summary>
    /// A value the caller says is free passes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Unique_ValueFree_Passes()
    {
        // arrange
        var validator = GetValidator<Registration>();

        // act
        var result = await validator.ValidateAsync(new Registration { Email = "free@example.com" });

        // assert
        result.HasErrors.IsFalse();
    }

    /// <summary>
    /// The synchronous overload behaves the same way.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Unique_Synchronous_Fails()
    {
        // arrange
        var validator = GetValidator<Handle>();

        // act
        var result = await validator.ValidateAsync(new Handle { Name = "taken" });

        // assert
        result.LabeledErrors.At(nameof(Handle.Name)).Has(1);
    }
}

/// <summary>
/// Model whose email must not already be registered.
/// </summary>
public class Registration
{
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Validator asking asynchronously whether the address is taken.
/// </summary>
public class RegistrationValidator : Validator<Registration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationValidator"/> class.
    /// </summary>
    public RegistrationValidator()
    {
        Field(x => x.Email).Unique((_, email) => Task.FromResult(email.StartsWith("taken")));
    }
}

/// <summary>
/// Model whose handle must not already be taken.
/// </summary>
public class Handle
{
    /// <summary>
    /// Gets or sets the handle.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Validator asking synchronously whether the handle is taken.
/// </summary>
public class HandleValidator : Validator<Handle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HandleValidator"/> class.
    /// </summary>
    public HandleValidator()
    {
        Field(x => x.Name).Unique((_, name) => name == "taken");
    }
}
