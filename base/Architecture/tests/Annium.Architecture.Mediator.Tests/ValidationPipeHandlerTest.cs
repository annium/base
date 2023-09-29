using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Architecture.Mediator.Tests;

public class ValidationPipeHandlerTest : TestBase
{
    public ValidationPipeHandlerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ValidationFailure_ReturnsBadRequest()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddValidationHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest();

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

        // assert
        result.Status.Is(OperationStatus.BadRequest);
        result.LabeledErrors.Has(2);
        result.LabeledErrors.At(nameof(LoginRequest.UserName)).Has(1);
        result.LabeledErrors.At(nameof(LoginRequest.Password)).Has(1);
    }

    [Fact]
    public async Task ValidationSuccess_ReturnsOriginalResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddValidationHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { UserName = "user", Password = "pass" };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
    }

    private class LoginRequest : IUserName, IPassword, IThrowing
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Throw => false;
    }

    private interface IUserName
    {
        string UserName { get; }
    }

    private interface IPassword
    {
        string Password { get; }
    }

    // ReSharper disable once UnusedType.Local
    private class UserNameValidator : Validator<IUserName>
    {
        public UserNameValidator()
        {
            Field(e => e.UserName).Required();
        }
    }

    // ReSharper disable once UnusedType.Local
    private class PasswordValidator : Validator<IPassword>
    {
        public PasswordValidator()
        {
            Field(e => e.Password).Required();
        }
    }
}