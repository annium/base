using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.DependencyInjection;
using Annium.Architecture.Mediator.Internal.PipeHandlers;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the validation pipe handler functionality.
/// </summary>
public class ValidationPipeHandlerTests : TestBase
{
    public ValidationPipeHandlerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterMediator(cfg => cfg.AddValidationHandler().AddHandler(typeof(EchoRequestHandler<>)));
    }

    /// <summary>
    /// Tests that validation failure returns a BadRequest status.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ValidationFailure_ReturnsBadRequest()
    {
        // arrange
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

    /// <summary>
    /// Tests that passing a null request to <c>ValidationPipeHandlerBase.HandleAsync</c> returns
    /// <see cref="OperationStatus.UncaughtError"/> with the "Request is empty" diagnostic message.
    /// Because <c>IMediator.SendAsync</c> dereferences <c>request.GetType()</c> before dispatching,
    /// null cannot travel through the normal mediator path; the handler is therefore exercised
    /// directly via reflection using a scoped instance resolved from the DI container, which is
    /// the same container wired by <c>RegisterMediator</c> in this test class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task NullRequest_ReturnsUncaughtError()
    {
        var ct = TestContext.Current.CancellationToken;

        // Locate the internal ValidationPipeHandler<LoginRequest, LoginRequest> type through the
        // public surface of the assembly (MediatorConfigurationExtensions is in the same assembly).
        var assembly = typeof(MediatorConfigurationExtensions).Assembly;
        var openHandlerType = assembly.GetType(
            "Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse.ValidationPipeHandler`2"
        );
        (openHandlerType is not null).IsTrue();
        var closedHandlerType = openHandlerType!.MakeGenericType(typeof(LoginRequest), typeof(LoginRequest));

        // Resolve the handler from a fresh DI scope (same lifetime as the mediator uses).
        await using var scope = CreateAsyncScope();
        var handler = scope.ServiceProvider.Resolve(closedHandlerType);

        // A dummy "next" that would be called only if the null guard does NOT fire.
        // It must never execute during this test.
        Task<IStatusResult<OperationStatus, LoginRequest>> DummyNext(LoginRequest _, CancellationToken __) =>
            throw new InvalidOperationException("next must not be reached when request is null");

        // Invoke HandleAsync(null!, ct, DummyNext) via reflection.
        var handleMethod = closedHandlerType.GetMethod(
            "HandleAsync",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            [
                typeof(LoginRequest),
                typeof(CancellationToken),
                typeof(Func<LoginRequest, CancellationToken, Task<IStatusResult<OperationStatus, LoginRequest>>>),
            ],
            null
        );
        (handleMethod is not null).IsTrue();
        var resultTask = (Task<IStatusResult<OperationStatus, LoginRequest>>)handleMethod!.Invoke(
            handler,
            [
                null,
                ct,
                (Func<LoginRequest, CancellationToken, Task<IStatusResult<OperationStatus, LoginRequest>>>)DummyNext,
            ]
        )!;

        // act
        var result = await resultTask;

        // assert
        result.Status.Is(OperationStatus.UncaughtError);
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is(PipeHandlerMessages.NullRequest);
    }
}
