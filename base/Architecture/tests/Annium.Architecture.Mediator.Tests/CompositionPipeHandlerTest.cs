using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the composition pipe handler functionality.
/// </summary>
public class CompositionPipeHandlerTest : TestBase
{
    public CompositionPipeHandlerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that composition failure returns a NotFound status.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CompositionFailure_ReturnsNotFound()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddCompositionHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { IsComposedSuccessfully = false };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.NotFound);
        result.LabeledErrors.Has(2);
        result.LabeledErrors.At(nameof(LoginRequest.UserName)).Has(1);
        result.LabeledErrors.At(nameof(LoginRequest.Password)).Has(1);
    }

    /// <summary>
    /// Tests that composition success returns the original result.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CompositionSuccess_ReturnsOriginalResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddCompositionHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { IsComposedSuccessfully = true };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
        result.Data.UserName.Is("username");
        result.Data.Password.Is("password");
    }

    /// <summary>
    /// Test request class for composition testing.
    /// </summary>
    private class LoginRequest : IUserName, IPassword, IThrowing
    {
        /// <summary>
        /// Gets or sets a value indicating whether composition should succeed.
        /// </summary>
        public bool IsComposedSuccessfully { get; set; }

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
    private interface IUserName : IFakeComposed
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        string UserName { get; set; }
    }

    /// <summary>
    /// Interface for objects that have a password.
    /// </summary>
    private interface IPassword : IFakeComposed
    {
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        string Password { get; set; }
    }

    /// <summary>
    /// Interface for objects that can be composed.
    /// </summary>
    private interface IFakeComposed
    {
        /// <summary>
        /// Gets or sets a value indicating whether composition should succeed.
        /// </summary>
        bool IsComposedSuccessfully { get; set; }
    }

    /// <summary>
    /// Composer for username fields.
    /// </summary>
    // ReSharper disable once UnusedType.Local
    private class UserNameComposer : Composer<IUserName>
    {
        public UserNameComposer()
        {
            Field(e => e.UserName).LoadWith(ctx => ctx.Root.IsComposedSuccessfully ? ctx.Label.ToLower() : null!);
        }
    }

    /// <summary>
    /// Composer for password fields.
    /// </summary>
    // ReSharper disable once UnusedType.Local
    private class PasswordComposer : Composer<IPassword>
    {
        public PasswordComposer()
        {
            Field(e => e.Password).LoadWith(ctx => ctx.Root.IsComposedSuccessfully ? ctx.Label.ToLower() : null!);
        }
    }
}
