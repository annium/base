using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the validation pipe handler functionality.
/// </summary>
public class ValidationPipeHandlerTest : TestBase
{
    public ValidationPipeHandlerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that validation failure returns a BadRequest status.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ValidationFailure_ReturnsBadRequest()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddValidationHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest();

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.BadRequest);
        result.LabeledErrors.Has(2);
        result.LabeledErrors.At(nameof(LoginRequest.UserName)).Has(1);
        result.LabeledErrors.At(nameof(LoginRequest.Password)).Has(1);
    }

    /// <summary>
    /// Tests that validation success returns the original result.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ValidationSuccess_ReturnsOriginalResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddValidationHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { UserName = "user", Password = "pass" };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
    }

    /// <summary>
    /// Test request class for validation testing.
    /// </summary>
    private class LoginRequest : IUserName, IPassword, IThrowing
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the handler should throw an exception.
        /// </summary>
        public bool Throw => false;
    }

    /// <summary>
    /// Interface for objects that have a username.
    /// </summary>
    private interface IUserName
    {
        /// <summary>
        /// Gets the username.
        /// </summary>
        string UserName { get; }
    }

    /// <summary>
    /// Interface for objects that have a password.
    /// </summary>
    private interface IPassword
    {
        /// <summary>
        /// Gets the password.
        /// </summary>
        string Password { get; }
    }

    /// <summary>
    /// Validator for username fields.
    /// </summary>
    // ReSharper disable once UnusedType.Local
    private class UserNameValidator : Validator<IUserName>
    {
        public UserNameValidator()
        {
            Field(e => e.UserName).Required();
        }
    }

    /// <summary>
    /// Validator for password fields.
    /// </summary>
    // ReSharper disable once UnusedType.Local
    private class PasswordValidator : Validator<IPassword>
    {
        public PasswordValidator()
        {
            Field(e => e.Password).Required();
        }
    }
}
