using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Architecture.Mediator.Tests;

public class CompositionPipeHandlerTest : TestBase
{
    public CompositionPipeHandlerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task CompositionFailure_ReturnsNotFound()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddCompositionHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { IsComposedSuccessfully = false };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

        // assert
        result.Status.Is(OperationStatus.NotFound);
        result.LabeledErrors.Has(2);
        result.LabeledErrors.At(nameof(LoginRequest.UserName)).Has(1);
        result.LabeledErrors.At(nameof(LoginRequest.Password)).Has(1);
    }

    [Fact]
    public async Task CompositionSuccess_ReturnsOriginalResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddCompositionHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { IsComposedSuccessfully = true };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
        result.Data.UserName.Is("username");
        result.Data.Password.Is("password");
    }

    private class LoginRequest : IUserName, IPassword, IThrowing
    {
        public bool IsComposedSuccessfully { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Throw => false;
    }

    private interface IUserName : IFakeComposed
    {
        string UserName { get; set; }
    }

    private interface IPassword : IFakeComposed
    {
        string Password { get; set; }
    }

    private interface IFakeComposed
    {
        bool IsComposedSuccessfully { get; set; }
    }

    // ReSharper disable once UnusedType.Local
    private class UserNameComposer : Composer<IUserName>
    {
        public UserNameComposer()
        {
            Field(e => e.UserName).LoadWith(ctx => ctx.Root.IsComposedSuccessfully ? ctx.Label.ToLower() : null!);
        }
    }

    // ReSharper disable once UnusedType.Local
    private class PasswordComposer : Composer<IPassword>
    {
        public PasswordComposer()
        {
            Field(e => e.Password).LoadWith(ctx => ctx.Root.IsComposedSuccessfully ? ctx.Label.ToLower() : null!);
        }
    }
}