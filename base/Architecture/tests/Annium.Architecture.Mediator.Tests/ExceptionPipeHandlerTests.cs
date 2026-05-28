using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.Mediator.Internal.PipeHandlers;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the exception pipe handler functionality.
/// </summary>
[Collection("LogConfigMutating")]
public class ExceptionPipeHandlerTests : TestBase
{
    public ExceptionPipeHandlerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        // Trace level is required so that ExceptionPipeHandlerBase.Failure logs are captured in
        // Logs for the assertions that verify which exception object (inner vs wrapper) was used.
        // OverrideLogLevel snapshots the prior level and restores it via TestBase.DisposeAsync;
        // the [Collection] attribute above serialises this class with the other LogConfig-mutating
        // test class so their global mutations don't race under xunit parallel-class execution.
        OverrideLogLevel(LogLevel.Trace);

        RegisterMediator(
            cfg =>
                cfg.AddExceptionHandler()
                    .AddHandler(typeof(DirectTargetInvocationHandler))
                    .AddHandler(typeof(EchoRequestHandler<>))
        );
    }

    /// <summary>
    /// Tests that exceptions are caught and returned as uncaught exception results.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Exception_ReturnsUncaughtExceptionResult()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new ThrowingRequest { Throw = true };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, ThrowingRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert: handler returns the generic sentinel; the raw exception message is intentionally
        // not surfaced to callers (it is logged separately via ExceptionPipeHandlerBase.Failure).
        result.Status.Is(OperationStatus.UncaughtError);
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is(PipeHandlerMessages.InternalError);
    }

    /// <summary>
    /// Tests that successful operations return the original result.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Success_ReturnsOriginalResult()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new ThrowingRequest { Throw = false };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, ThrowingRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
    }

    /// <summary>
    /// Tests that when the downstream handler throws via reflection (producing a
    /// <see cref="TargetInvocationException"/> wrapper), the handler unwraps it and
    /// logs the inner exception rather than the outer wrapper.
    /// The mediator invokes handlers through reflection, so every exception thrown by a
    /// downstream handler arrives wrapped in <see cref="TargetInvocationException"/>;
    /// the base class must peel that wrapper before logging and returning.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task TargetInvocationException_WithInnerException_LogsInnerNotWrapper()
    {
        // arrange — Throw=true makes EchoRequestHandler throw InvalidOperationException,
        // which the mediator's reflection infrastructure wraps in TargetInvocationException.
        var mediator = Get<IMediator>();
        var request = new ThrowingRequest { Throw = true };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, ThrowingRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert — outer status
        result.Status.Is(OperationStatus.UncaughtError);
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is(PipeHandlerMessages.InternalError);

        // assert — the Failure trace must carry the INNER exception (InvalidOperationException),
        // not the TargetInvocationException wrapper; if the unwrapping branch were missing the
        // Data["exception"] would be a TargetInvocationException and this assertion would fail.
        var failureLog = Logs.SingleOrDefault(m => m.MessageTemplate == "Failure of {request}: {exception}");
        (failureLog is not null).IsTrue();
        (failureLog!.Data["exception"] is not null).IsTrue();
        (failureLog.Data["exception"] is InvalidOperationException).IsTrue();
        (failureLog.Data["exception"] is TargetInvocationException).IsFalse();
    }

    /// <summary>
    /// Tests that when a handler directly throws a <see cref="TargetInvocationException"/>
    /// whose <c>InnerException</c> is <c>null</c>, the base class falls back to logging the
    /// wrapper itself (the <c>exception.InnerException ?? exception</c> null-coalescing branch).
    /// Because the handler runs inside the mediator's own reflection invocation chain, the
    /// directly-thrown <see cref="TargetInvocationException"/> arrives as the <c>InnerException</c>
    /// of a second wrapper; the branch under test therefore uses that unwrapped
    /// <see cref="TargetInvocationException"/> (which has a <c>null</c> InnerException) as the
    /// fallback exception reported to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task TargetInvocationException_WithNullInnerException_UsesFallbackWrapper()
    {
        // arrange — DirectTargetInvocationHandler throws new TargetInvocationException(msg, null).
        // The mediator's reflection layer wraps that in a second TargetInvocationException;
        // the catch clause unwraps once, landing on the directly-thrown TIE (InnerException==null),
        // which is then the exception passed to Failure via the ?? fallback.
        var mediator = Get<IMediator>();
        var request = new NullInnerRequest();

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, NullInnerRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert — outer status
        result.Status.Is(OperationStatus.UncaughtError);
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is(PipeHandlerMessages.InternalError);

        // assert — the Failure trace carries the TargetInvocationException that was thrown
        // directly by DirectTargetInvocationHandler; its InnerException is null so the
        // ?? fallback kept the wrapper.  The logged exception must therefore be a
        // TargetInvocationException (not an inner cause) and its InnerException must be null.
        var failureLog = Logs.SingleOrDefault(m => m.MessageTemplate == "Failure of {request}: {exception}");
        (failureLog is not null).IsTrue();
        (failureLog!.Data["exception"] is not null).IsTrue();
        (failureLog.Data["exception"] is TargetInvocationException).IsTrue();
        (((TargetInvocationException)failureLog.Data["exception"]!).InnerException is null).IsTrue();
    }

    /// <summary>
    /// Request type used to exercise the null-InnerException fallback branch of
    /// <c>ExceptionPipeHandlerBase</c>. Handled exclusively by
    /// <see cref="DirectTargetInvocationHandler"/>.
    /// </summary>
    private class NullInnerRequest : IThrowing
    {
        /// <summary>
        /// Gets a value indicating whether an exception should be thrown; always false for this request.
        /// </summary>
        public bool Throw => false;
    }

    /// <summary>
    /// Final handler for <see cref="NullInnerRequest"/> that deliberately throws a
    /// <see cref="TargetInvocationException"/> with a <c>null</c> inner exception, exercising the
    /// <c>exception.InnerException ?? exception</c> null-coalescing fallback in
    /// <c>ExceptionPipeHandlerBase.HandleAsync</c>.
    /// </summary>
    private class DirectTargetInvocationHandler
        : IFinalRequestHandler<NullInnerRequest, IStatusResult<OperationStatus, NullInnerRequest>>
    {
        /// <summary>
        /// Throws a <see cref="TargetInvocationException"/> with no inner exception.
        /// </summary>
        /// <param name="request">The request (unused).</param>
        /// <param name="ct">The cancellation token (unused).</param>
        /// <returns>Never returns normally.</returns>
        public Task<IStatusResult<OperationStatus, NullInnerRequest>> HandleAsync(
            NullInnerRequest request,
            CancellationToken ct
        ) => throw new TargetInvocationException("deliberate bare wrapper", null);
    }
}
