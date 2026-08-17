using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Data.Operations.Serialization.Json;
using Annium.Logging;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Core.Mediator.Tests;

/// <summary>
/// Tests for mediator functionality. Each test materializes its own <see cref="Fixture"/> so that
/// per-test mediator configurations don't collide with the registration window enforced by
/// <see cref="Annium.Testing.TestBase"/>.
/// </summary>
public class MediatorTest
{
    /// <summary>JSON serializer options configured for operations serialization, shared across the test handlers and wrappers.</summary>
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForOperations();

    /// <summary>The test output helper used for logging within this test class.</summary>
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public MediatorTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    /// <summary>
    /// Tests that a single closed handler works correctly.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SingleClosedHandler_Works()
    {
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));

        var mediator = fx.Get<IMediator>();
        var request = new Base { Value = "base" };

        var response = await mediator.SendAsync<One>(request, TestContext.Current.CancellationToken);

        AssertClosedHandlerResult(response, request);
    }

    /// <summary>
    /// Tests that a single open handler works correctly with expected parameters.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SingleOpenHandler_WithExpectedParameters_Works()
    {
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(OpenFinalHandler<,>)));

        var mediator = fx.Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };

        var response = await mediator.SendAsync<Base>(request, TestContext.Current.CancellationToken);

        response.Value.Is("one_two_three");
    }

    /// <summary>
    /// Tests that a chain of handlers works correctly with expected parameters.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ChainOfHandlers_WithExpectedParameters_Works()
    {
        await using var fx = await BuildAsync(cfg => AddPipeChain(cfg));

        var mediator = fx.Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        var response = (
            await mediator.SendAsync<Response<IBooleanResult<Base>>>(payload, TestContext.Current.CancellationToken)
        ).Value;

        AssertPipeSuccess(response);
    }

    /// <summary>
    /// Tests that a chain of handlers works correctly with registered responses.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ChainOfHandlers_WithRegisteredResponse_Works()
    {
        await using var fx = await BuildAsync(cfg =>
            AddPipeChain(cfg).AddMatch(typeof(Request<Two>), typeof(IResponse), typeof(Response<IBooleanResult<Base>>))
        );

        var mediator = fx.Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        var response = (await mediator.SendAsync<IResponse>(payload, TestContext.Current.CancellationToken))
            .As<Response<IBooleanResult<Base>>>()
            .Value;

        AssertPipeSuccess(response);
    }

    // -------------------------------------------------------------------------
    // GROUP A — MediatorConfiguration validation (throws synchronously during configure(cfg))
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers the standard three-handler pipe chain (ConversionHandler → ValidationHandler → OpenFinalHandler)
    /// onto <paramref name="cfg"/> and returns it for further chaining.
    /// </summary>
    /// <param name="cfg">The mediator configuration to add the handlers to.</param>
    /// <returns>The same configuration instance for method chaining.</returns>
    private static MediatorConfiguration AddPipeChain(MediatorConfiguration cfg) =>
        cfg.AddHandler(typeof(ConversionHandler<,>))
            .AddHandler(typeof(ValidationHandler<,>))
            .AddHandler(typeof(OpenFinalHandler<,>));

    /// <summary>
    /// Asserts that <paramref name="response"/> represents a successful pipe result with the
    /// canonicalized "one_two_three" value.
    /// </summary>
    /// <param name="response">The boolean result wrapping the <see cref="Base"/> response.</param>
    private static void AssertPipeSuccess(IBooleanResult<Base> response)
    {
        response.IsSuccess.IsTrue();
        response.Data.Value.Is("one_two_three");
    }

    /// <summary>
    /// Asserts that a <see cref="ClosedFinalHandler"/> response correctly reflects the originating
    /// <paramref name="request"/>: the value is preserved and <see cref="One.First"/> equals the
    /// value's character count.
    /// </summary>
    /// <param name="response">The mapped <see cref="One"/> response.</param>
    /// <param name="request">The originating <see cref="Base"/> request.</param>
    private static void AssertClosedHandlerResult(One response, Base request)
    {
        response.Value.Is(request.Value);
        response.First.Is((long)request.Value.NotNull().Length);
    }

    /// <summary>
    /// Asserts that applying <paramref name="configure"/> to a fresh configuration throws
    /// <see cref="InvalidOperationException"/>. The configure action runs synchronously inside
    /// <c>AddMediatorConfiguration</c>, so the guard exception propagates immediately.
    /// </summary>
    /// <param name="configure">The configuration action expected to throw.</param>
    private static void ThrowsOnConfigure(Action<MediatorConfiguration> configure) =>
        Wrap.It(() => new ServiceContainer().AddMediatorConfiguration(configure)).Throws<InvalidOperationException>();

    /// <summary>
    /// Tests that registering a non-handler type as a mediator handler throws.
    /// </summary>
    [Fact]
    public void AddHandler_NonHandlerType_Throws() => ThrowsOnConfigure(cfg => cfg.AddHandler(typeof(object)));

    /// <summary>
    /// Tests that adding a match with a generic requested type throws.
    /// </summary>
    [Fact]
    public void AddMatch_GenericRequestedType_Throws() =>
        ThrowsOnConfigure(cfg => cfg.AddMatch(typeof(System.Collections.Generic.List<>), typeof(Base), typeof(One)));

    /// <summary>
    /// Tests that adding a match with a generic expected type throws.
    /// </summary>
    [Fact]
    public void AddMatch_GenericExpectedType_Throws() =>
        ThrowsOnConfigure(cfg => cfg.AddMatch(typeof(Two), typeof(System.Collections.Generic.List<>), typeof(One)));

    /// <summary>
    /// Tests that adding a match with a generic resolved type throws.
    /// </summary>
    [Fact]
    public void AddMatch_GenericResolvedType_Throws() =>
        ThrowsOnConfigure(cfg => cfg.AddMatch(typeof(Two), typeof(Base), typeof(System.Collections.Generic.List<>)));

    /// <summary>
    /// Tests that AddMatch throws when the resolved type is not assignable to the expected type.
    /// Two/IResponse/One are all non-generic (generic guards pass); One does not implement IResponse,
    /// so the assignability guard fires and throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void AddMatch_ResolvedTypeNotAssignableToExpected_Throws() =>
        ThrowsOnConfigure(cfg => cfg.AddMatch(typeof(Two), typeof(IResponse), typeof(One)));

    /// <summary>
    /// Tests that registering two matches with the same requested/expected pair but different resolved types throws.
    /// </summary>
    [Fact]
    public void AddMatch_AmbiguousDuplicate_Throws() =>
        ThrowsOnConfigure(cfg =>
        {
            cfg.AddMatch(typeof(Two), typeof(Base), typeof(One));
            cfg.AddMatch(typeof(Two), typeof(Base), typeof(Two));
        });

    /// <summary>
    /// Tests that passing a bare type parameter (e.g. the T from <c>List&lt;&gt;</c>) as the requested
    /// type to <see cref="MediatorConfiguration.AddMatch"/> throws because
    /// <c>ThrowIfGeneric</c> detects <c>IsGenericTypeParameter == true</c>.
    /// </summary>
    [Fact]
    public void AddMatch_BareTypeParameterRequested_Throws() =>
        ThrowsOnConfigure(cfg =>
            cfg.AddMatch(typeof(System.Collections.Generic.List<>).GetGenericArguments()[0], typeof(Base), typeof(One))
        );

    /// <summary>
    /// Tests that registering the same match twice within one config is silently deduplicated: it neither
    /// throws at configure time nor produces a duplicate that would make ChainBuilder's SingleOrDefault throw at dispatch.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task AddMatch_IdenticalDuplicate_Accepted()
    {
        // The same (Request<Two>, IResponse, Response<...>) match is registered twice in ONE config.
        // If AddMatch did not dedupe, two identical matches would survive and ResolveOutput's
        // SingleOrDefault would throw "Sequence contains more than one element" at dispatch.
        await using var fx = await BuildAsync(cfg =>
            AddPipeChain(cfg)
                .AddMatch(typeof(Request<Two>), typeof(IResponse), typeof(Response<IBooleanResult<Base>>))
                .AddMatch(typeof(Request<Two>), typeof(IResponse), typeof(Response<IBooleanResult<Base>>))
        );

        var mediator = fx.Get<IMediator>();
        var payload = new Request<Two>(new Two { Second = 2, Value = "one two three" });

        var response = (await mediator.SendAsync<IResponse>(payload, TestContext.Current.CancellationToken))
            .As<Response<IBooleanResult<Base>>>()
            .Value;

        AssertPipeSuccess(response);
    }

    // -------------------------------------------------------------------------
    // GROUP B — Merge across two AddMediatorConfiguration calls (conflict detected at provider build)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that two configurations with conflicting matches for the same requested/expected pair throw during provider build.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task TwoConfigurations_ConflictingMatch_Throws()
    {
        await using var fx = new Fixture(_outputHelper);
        // Override the standard Configure path — register two conflicting configs manually.
        fx.Register(container =>
        {
            Fixture.RegisterValidators(container);
            container.AddMediatorConfiguration(cfg =>
                cfg.AddHandler(typeof(ClosedFinalHandler)).AddMatch(typeof(Two), typeof(Base), typeof(One))
            );
            container.AddMediatorConfiguration(cfg => cfg.AddMatch(typeof(Two), typeof(Base), typeof(Two)));
            container.AddMediator();
        });
        fx.Setup(sp =>
        {
            sp.UseLogging(route =>
                route.For(m => m.SubjectType.StartsWith("ClosedFinalHandler")).UseInMemory<DefaultLogContext>()
            );
        });
        await fx.InitializeAsync();

        // ChainBuilder.Merge runs in the ChainBuilder constructor, which the singleton IMediator
        // instantiates on first resolve — that is where the cross-config match conflict surfaces.
        Wrap.It(() => fx.Get<IMediator>()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that two configurations registering the same match are silently deduplicated and the pipeline works.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task TwoConfigurations_IdenticalMatch_Works()
    {
        await using var fx = new Fixture(_outputHelper);
        var matchArgs = (typeof(Request<Two>), typeof(IResponse), typeof(Response<IBooleanResult<Base>>));
        fx.Register(container =>
        {
            Fixture.RegisterValidators(container);
            container.AddMediatorConfiguration(cfg =>
                AddPipeChain(cfg).AddMatch(matchArgs.Item1, matchArgs.Item2, matchArgs.Item3)
            );
            // Second config registers the identical match — should be deduplicated without error.
            container.AddMediatorConfiguration(cfg => cfg.AddMatch(matchArgs.Item1, matchArgs.Item2, matchArgs.Item3));
            container.AddMediator();
        });
        fx.Setup(sp =>
        {
            sp.UseLogging(route =>
                route
                    .For(m =>
                        m.SubjectType.StartsWith("ConversionHandler")
                        || m.SubjectType.StartsWith("ValidationHandler")
                        || m.SubjectType.StartsWith("OpenFinalHandler")
                    )
                    .UseInMemory<DefaultLogContext>()
            );
        });
        await fx.InitializeAsync();

        var mediator = fx.Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        var response = (await mediator.SendAsync<IResponse>(payload, TestContext.Current.CancellationToken))
            .As<Response<IBooleanResult<Base>>>()
            .Value;

        AssertPipeSuccess(response);
    }

    /// <summary>
    /// Tests that Merge concatenates handlers contributed by SEPARATE configurations into one chain:
    /// config 1 registers the pipe handlers, config 2 registers the final handler, and dispatch builds
    /// a complete chain across both configs and succeeds.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task TwoConfigurations_HandlersSplitAcrossConfigs_DispatchSucceeds()
    {
        await using var fx = new Fixture(_outputHelper);
        fx.Register(container =>
        {
            Fixture.RegisterValidators(container);
            // config 1 contributes only the pipe handlers
            container.AddMediatorConfiguration(cfg =>
                cfg.AddHandler(typeof(ConversionHandler<,>)).AddHandler(typeof(ValidationHandler<,>))
            );
            // config 2 contributes only the final handler
            container.AddMediatorConfiguration(cfg => cfg.AddHandler(typeof(OpenFinalHandler<,>)));
            container.AddMediator();
        });
        fx.Setup(sp =>
        {
            sp.UseLogging(route =>
                route
                    .For(m =>
                        m.SubjectType.StartsWith("ConversionHandler")
                        || m.SubjectType.StartsWith("ValidationHandler")
                        || m.SubjectType.StartsWith("OpenFinalHandler")
                    )
                    .UseInMemory<DefaultLogContext>()
            );
        });
        await fx.InitializeAsync();

        var mediator = fx.Get<IMediator>();
        var payload = new Request<Two>(new Two { Second = 2, Value = "one two three" });

        var response = (
            await mediator.SendAsync<Response<IBooleanResult<Base>>>(payload, TestContext.Current.CancellationToken)
        ).Value;

        AssertPipeSuccess(response);
    }

    // -------------------------------------------------------------------------
    // GROUP C — Pipeline behavior
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that a cancelled token propagates OperationCanceledException through the pipe-handler branch
    /// (hasNext=true in ChainExecutor), exercising DoNotWrapExceptions on the pipe handler invocation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_CancelledToken_PipeHandlerThrows_PropagatesOperationCanceledException()
    {
        // Register pipe handler FIRST so the chain is [pipe, final] for Base → One.
        await using var fx = await BuildAsync(cfg =>
            cfg.AddHandler(typeof(CancelObservingPipeHandler)).AddHandler(typeof(ClosedFinalHandler))
        );

        var mediator = fx.Get<IMediator>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () => await mediator.SendAsync<One>(new Base { Value = "base" }, cts.Token))
            .ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Tests that the default SendAsync overload opens a fresh AsyncScope per call so a Scoped
    /// handler resolves a distinct instance for each invocation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_DefaultOverload_CreatesFreshScopePerCall()
    {
        await using var fx = new Fixture(_outputHelper);
        fx.Register(container =>
        {
            Fixture.RegisterValidators(container);
            container.Add(new InstanceCollector()).AsSelf().Singleton();
            container.AddMediatorConfiguration(cfg => cfg.AddHandler(typeof(ScopeProbeHandler)));
            container.AddMediator();
        });
        fx.Setup(sp =>
            sp.UseLogging(route =>
                route.For(m => m.SubjectType.StartsWith("ScopeProbeHandler")).UseInMemory<DefaultLogContext>()
            )
        );
        await fx.InitializeAsync();

        var mediator = fx.Get<IMediator>();
        var ct = TestContext.Current.CancellationToken;

        await mediator.SendAsync<One>(new Base { Value = "base" }, ct);
        await mediator.SendAsync<One>(new Base { Value = "base" }, ct);

        var collector = fx.Get<InstanceCollector>();
        // Two calls must have produced exactly two handler invocations.
        collector.Ids.Has(2);
        // Each call opened a fresh scope, so each call created a fresh ScopeProbeHandler instance.
        (collector.Ids[0] != collector.Ids[1]).IsTrue();
    }

    /// <summary>
    /// Tests that dispatching with an already-cancelled token propagates OperationCanceledException.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(CancelObservingFinalHandler)));

        var mediator = fx.Get<IMediator>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () => await mediator.SendAsync<One>(new Base { Value = "base" }, cts.Token))
            .ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Tests that sending a request for which no handler is registered throws.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_NoHandlerRegistered_Throws()
    {
        // Register a handler that covers Base->One but try to send One->Two (unresolvable).
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));

        var mediator = fx.Get<IMediator>();

        await Wrap.It(async () =>
                await mediator.SendAsync<Two>(new One { Value = "x", First = 1 }, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that two AddMediatorConfiguration calls each registering the same handler type still dispatch correctly.
    /// MediatorConfiguration.Merge concatenates handlers without deduplication, so two identical Handler entries
    /// exist after the merge; ChainBuilder uses the first match and the duplicate is harmlessly ignored.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task TwoConfigurations_DuplicateHandler_DispatchSucceeds()
    {
        await using var fx = new Fixture(_outputHelper);
        fx.Register(container =>
        {
            Fixture.RegisterValidators(container);
            container.AddMediatorConfiguration(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));
            container.AddMediatorConfiguration(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));
            container.AddMediator();
        });
        fx.Setup(sp =>
        {
            sp.UseLogging(route =>
                route.For(m => m.SubjectType.StartsWith("ClosedFinalHandler")).UseInMemory<DefaultLogContext>()
            );
        });
        await fx.InitializeAsync();

        var mediator = fx.Get<IMediator>();
        var request = new Base { Value = "base" };

        var response = await mediator.SendAsync<One>(request, TestContext.Current.CancellationToken);

        AssertClosedHandlerResult(response, request);
    }

    /// <summary>
    /// Tests that dispatching a request that matches a pipe handler, but whose transformed inner types have no
    /// registered final handler, throws <see cref="InvalidOperationException"/> at chain-build time.
    /// ConversionHandler&lt;Two,Base&gt; matches (Request&lt;Two&gt;, Response&lt;Base&gt;) and advances the chain
    /// to (Two, Base); since no handler covers (Two, Base), ChainBuilder throws before any handler body runs.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_PipeHandlerWithNoFinalForTransformedType_Throws()
    {
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(ConversionHandler<,>)));

        var mediator = fx.Get<IMediator>();
        var payload = new Request<Two>(new Two { Second = 2, Value = "one two three" });

        await Wrap.It(async () =>
                await mediator.SendAsync<Response<Base>>(payload, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that a chain of handlers returns a failure result when validation fails.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ChainOfHandlers_ValidationFails_ReturnsFailure()
    {
        await using var fx = await BuildAsync(cfg => AddPipeChain(cfg));

        var mediator = fx.Get<IMediator>();
        // Second = 1 (odd) — Func<Two,bool> evaluates to false, ValidationHandler returns failure.
        var request = new Two { Second = 1, Value = "one two three" };
        var payload = new Request<Two>(request);

        var response = (
            await mediator.SendAsync<Response<IBooleanResult<Base>>>(payload, TestContext.Current.CancellationToken)
        ).Value;

        response.IsSuccess.IsFalse();
        response.HasErrors.IsTrue();
    }

    /// <summary>
    /// Tests that SendAsync with an explicit IServiceProvider overload produces the same result.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_WithExplicitServiceProvider_Works()
    {
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));

        var mediator = fx.Get<IMediator>();
        var request = new Base { Value = "base" };

        // bring-your-own-scope overload: the caller owns the scope and the mediator dispatches against
        // the supplied provider directly (no nested scope). Exercise it with a real explicit scope.
        await using var scope = fx.Provider.CreateAsyncScope();
        var response = await mediator.SendAsync<One>(
            scope.ServiceProvider,
            request,
            TestContext.Current.CancellationToken
        );

        AssertClosedHandlerResult(response, request);
    }

    /// <summary>
    /// Tests that calling SendAsync twice reuses the cached chain and both calls succeed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_SameTypeTwice_BothSucceed()
    {
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));

        var mediator = fx.Get<IMediator>();
        var request = new Base { Value = "base" };
        var ct = TestContext.Current.CancellationToken;

        var first = await mediator.SendAsync<One>(request, ct);
        var second = await mediator.SendAsync<One>(request, ct);

        AssertClosedHandlerResult(first, request);
        AssertClosedHandlerResult(second, request);
    }

    /// <summary>
    /// Tests that calling <c>AddMediator</c> with no <c>AddMediatorConfiguration</c> at all (so ChainBuilder
    /// merges an empty configuration set) throws <see cref="InvalidOperationException"/> at first dispatch
    /// rather than at container-build time.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_AddMediatorWithoutConfiguration_Throws()
    {
        await using var fx = new Fixture(_outputHelper);
        fx.Register(container =>
        {
            Fixture.RegisterValidators(container);
            // intentionally no AddMediatorConfiguration → ChainBuilder receives an empty config set
            container.AddMediator();
        });
        await fx.InitializeAsync();

        var mediator = fx.Get<IMediator>();

        await Wrap.It(async () =>
                await mediator.SendAsync<One>(new Base { Value = "base" }, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that a non-cancellation exception thrown by a handler propagates as its ORIGINAL type
    /// (e.g. <see cref="FormatException"/>), not wrapped in a <c>TargetInvocationException</c> — confirming
    /// ChainExecutor invokes handlers with <c>BindingFlags.DoNotWrapExceptions</c> for arbitrary exception types
    /// (the cancellation tests alone don't prove this, since OperationCanceledException can unwrap specially).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_HandlerThrowsNonCancellationException_PropagatesUnwrapped()
    {
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(ThrowingFinalHandler)));

        var mediator = fx.Get<IMediator>();

        await Wrap.It(async () =>
                await mediator.SendAsync<One>(new Base { Value = "base" }, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<FormatException>();
    }

    /// <summary>
    /// Tests that when <see cref="MediatorConfiguration.AddMatch"/> remaps the output type but no handler is
    /// registered for the resulting (request, resolved-response) pair, <c>SendAsync</c>
    /// throws <see cref="InvalidOperationException"/>.
    /// ChainBuilder.ResolveOutput successfully maps <c>IResponse</c> to <c>Response&lt;IBooleanResult&lt;Base&gt;&gt;</c>,
    /// then the handler loop finds no match and throws before any handler body runs.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_MatchRemappedOutputHasNoHandler_Throws()
    {
        await using var fx = await BuildAsync(cfg =>
            cfg.AddMatch(typeof(Request<Two>), typeof(IResponse), typeof(Response<IBooleanResult<Base>>))
        );

        var mediator = fx.Get<IMediator>();
        var payload = new Request<Two>(new Two { Second = 2, Value = "x" });

        await Wrap.It(async () => await mediator.SendAsync<IResponse>(payload, TestContext.Current.CancellationToken))
            .ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that a passthrough pipe handler (same input and output types as the final handler) is placed
    /// first in the chain and correctly delegates to the closed final handler, returning the expected result.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_PassthroughSameTypePipe_ReturnsFinalResult()
    {
        await using var fx = await BuildAsync(cfg =>
            cfg.AddHandler(typeof(PassthroughPipeHandler)).AddHandler(typeof(ClosedFinalHandler))
        );

        var mediator = fx.Get<IMediator>();
        var request = new Base { Value = "x" };

        var response = await mediator.SendAsync<One>(request, TestContext.Current.CancellationToken);

        AssertClosedHandlerResult(response, request);
    }

    /// <summary>
    /// Builds an initialized fixture with the supplied mediator configuration.
    /// </summary>
    /// <param name="configure">Action that configures the mediator registrations.</param>
    /// <returns>A task that resolves to the initialized <see cref="Fixture"/>.</returns>
    private async Task<Fixture> BuildAsync(Action<MediatorConfiguration> configure)
    {
        var fx = new Fixture(_outputHelper);
        fx.Configure(configure);
        await fx.InitializeAsync();
        return fx;
    }

    /// <summary>
    /// Per-test fixture inheriting <see cref="Annium.Testing.TestBase"/>; configures the mediator
    /// registrations once via <see cref="Configure"/> before <see cref="Annium.Testing.TestBase.InitializeAsync"/>
    /// is invoked.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    private sealed class Fixture(ITestOutputHelper outputHelper) : TestBase(outputHelper), IAsyncDisposable
    {
        /// <summary>
        /// Registers the request validators (<see cref="Func{One, Boolean}"/> / <see cref="Func{Two, Boolean}"/>)
        /// that <see cref="ValidationHandler{TRequest, TResponse}"/> resolves. Shared by every test fixture setup.
        /// </summary>
        /// <param name="container">The service container to register into.</param>
        public static void RegisterValidators(IServiceContainer container)
        {
            container.Add<Func<One, bool>>(value => value.First % 2 == 1).AsSelf().Singleton();
            container.Add<Func<Two, bool>>(value => value.Second % 2 == 0).AsSelf().Singleton();
        }

        /// <summary>
        /// Registers mediator handlers + validators + logging routes. Must be called before
        /// <see cref="Annium.Testing.TestBase.InitializeAsync"/>.
        /// </summary>
        /// <param name="configure">Action that configures the mediator registrations.</param>
        public void Configure(Action<MediatorConfiguration> configure)
        {
            Register(container =>
            {
                RegisterValidators(container);
                container.AddMediatorConfiguration(configure);
                container.AddMediator();
            });
            Setup(sp =>
            {
                sp.UseLogging(route =>
                    route
                        .For(m =>
                            m.SubjectType.StartsWith("ConversionHandler")
                            || m.SubjectType.StartsWith("ValidationHandler")
                            || m.SubjectType.StartsWith("OpenFinalHandler")
                            || m.SubjectType.StartsWith("ClosedFinalHandler")
                            || m.SubjectType.StartsWith("PassthroughPipeHandler")
                        )
                        .UseInMemory<DefaultLogContext>()
                );
            });
        }
    }

    /// <summary>
    /// Handler that converts between request and response types using JSON serialization.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    private class ConversionHandler<TRequest, TResponse>
        : IPipeRequestHandler<Request<TRequest>, TRequest, TResponse, Response<TResponse>>,
            ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversionHandler{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public ConversionHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Deserializes the outer request, delegates to the next handler, then wraps the result in a response.
        /// </summary>
        /// <param name="request">The wrapped request containing a JSON-serialized payload.</param>
        /// <param name="ct">A cancellation token to observe while awaiting the next handler.</param>
        /// <param name="next">The next handler delegate in the pipeline.</param>
        /// <returns>A task that resolves to the serialized response wrapper.</returns>
        public async Task<Response<TResponse>> HandleAsync(
            Request<TRequest> request,
            CancellationToken ct,
            Func<TRequest, CancellationToken, Task<TResponse>> next
        )
        {
            this.Trace<string>("Deserialize Request to {request}", typeof(TRequest).FriendlyName());
            // test payloads always serialize to non-null JSON; TRequest is unconstrained so NotNull() can't bind here
            var payload = JsonSerializer.Deserialize<TRequest>(request.Value, _options)!;

            var result = await next(payload, ct);

            this.Trace<string>("Serialize {response} to Response", typeof(TResponse).FriendlyName());
            return new Response<TResponse>(JsonSerializer.Serialize(result, _options));
        }
    }

    /// <summary>Request wrapper that serializes the value using JSON.</summary>
    /// <typeparam name="T">The type of the request value.</typeparam>
    private class Request<T>
    {
        /// <summary>Gets the serialized value.</summary>
        public string Value { get; }

        /// <summary>Initializes a new instance of the <see cref="Request{T}"/> class.</summary>
        /// <param name="value">Payload carried by the request.</param>
        public Request(T value)
        {
            Value = JsonSerializer.Serialize(value, _options);
        }
    }

    /// <summary>Response wrapper that deserializes the value using JSON.</summary>
    /// <typeparam name="T">The type of the response value.</typeparam>
    private class Response<T> : IResponse
    {
        /// <summary>Gets the deserialized value.</summary>
        public T Value { get; }

        /// <summary>Initializes a new instance of the <see cref="Response{T}"/> class.</summary>
        /// <param name="value">Payload carried by the response.</param>
        public Response(string value)
        {
            // test payloads always serialize to non-null JSON; T is unconstrained so NotNull() can't bind here
            Value = JsonSerializer.Deserialize<T>(value, _options)!;
        }
    }

    /// <summary>Marker interface for response types.</summary>
    private interface IResponse;

    /// <summary>Handler that validates requests before processing.</summary>
    private class ValidationHandler<TRequest, TResponse>
        : IPipeRequestHandler<TRequest, TRequest, TResponse, IBooleanResult<TResponse>>,
            ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>The delegate used to validate incoming requests.</summary>
        private readonly Func<TRequest, bool> _validate;

        /// <summary>Initializes a new instance of the <see cref="ValidationHandler{TRequest, TResponse}"/> class.</summary>
        /// <param name="validate">Predicate deciding whether the request passes validation.</param>
        /// <param name="logger">Logger used for tracing.</param>
        public ValidationHandler(Func<TRequest, bool> validate, ILogger logger)
        {
            _validate = validate;
            Logger = logger;
        }

        /// <summary>
        /// Validates the request and, if valid, delegates to the next handler; returns a failure result on validation errors.
        /// </summary>
        /// <param name="request">The request to validate and process.</param>
        /// <param name="ct">A cancellation token to observe while awaiting the next handler.</param>
        /// <param name="next">The next handler delegate in the pipeline.</param>
        /// <returns>A task that resolves to a boolean result wrapping the response, indicating success or validation failure.</returns>
        public async Task<IBooleanResult<TResponse>> HandleAsync(
            TRequest request,
            CancellationToken ct,
            Func<TRequest, CancellationToken, Task<TResponse>> next
        )
        {
            this.Trace<string>("Start {request} validation", typeof(TRequest).FriendlyName());
            // default(TResponse)! is a placeholder: Data is discarded on success (overwritten below) and unused on failure
            var result = _validate(request)
                ? Result.Success(default(TResponse)!)
                : Result.Failure(default(TResponse)!).Error("Validation failed");
            this.Trace(
                "Status of {request} validation: {isSuccess}",
                typeof(TRequest).FriendlyName(),
                result.IsSuccess
            );
            if (result.HasErrors)
                return result;

            var response = await next(request, ct);

            return Result.Success(response);
        }
    }

    /// <summary>Final handler for open generic requests that transforms the request value.</summary>
    private class OpenFinalHandler<TRequest, TResponse> : IFinalRequestHandler<TRequest, TResponse>, ILogSubject
        where TRequest : TResponse
        where TResponse : Base, new()
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="OpenFinalHandler{TRequest, TResponse}"/> class.</summary>
        /// <param name="logger">Logger used for tracing.</param>
        public OpenFinalHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Handles the request by replacing spaces in the value with underscores and returning the response.
        /// </summary>
        /// <param name="request">The request whose value will be transformed.</param>
        /// <param name="ct">A cancellation token (unused but required by the interface).</param>
        /// <returns>A task that resolves to the transformed response.</returns>
        public Task<TResponse> HandleAsync(TRequest request, CancellationToken ct)
        {
            this.Info<string>("handler: {type}", GetType().FriendlyName());
            this.Trace<int>("request hash: {hash}", request.GetHashCode());

            var response = new TResponse { Value = request.Value.NotNull().Replace(' ', '_') };

            return Task.FromResult(response);
        }
    }

    /// <summary>Final handler for closed requests that converts Base to One.</summary>
    private class ClosedFinalHandler : IFinalRequestHandler<Base, One>, ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="ClosedFinalHandler"/> class.</summary>
        /// <param name="logger">Logger used for tracing.</param>
        public ClosedFinalHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Handles the request by mapping a <see cref="Base"/> instance to a <see cref="One"/> response.
        /// </summary>
        /// <param name="request">The base request containing the value to map.</param>
        /// <param name="ct">A cancellation token (unused but required by the interface).</param>
        /// <returns>A task that resolves to the mapped <see cref="One"/> response.</returns>
        public Task<One> HandleAsync(Base request, CancellationToken ct)
        {
            this.Trace<string>("handler: {type}", GetType().FullName.NotNull());
            this.Trace<int>("request hash: {hash}", request.GetHashCode());

            return Task.FromResult(MapBaseToOne(request));
        }
    }

    /// <summary>Final handler that throws a non-cancellation exception, to verify unwrapped exception propagation.</summary>
    private class ThrowingFinalHandler : IFinalRequestHandler<Base, One>, ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="ThrowingFinalHandler"/> class.</summary>
        /// <param name="logger">Logger used for tracing.</param>
        public ThrowingFinalHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>Always throws <see cref="FormatException"/> to exercise unwrapped exception propagation.</summary>
        /// <param name="request">The base request (unused).</param>
        /// <param name="ct">A cancellation token (unused).</param>
        /// <returns>Never returns; always throws.</returns>
        public Task<One> HandleAsync(Base request, CancellationToken ct) => throw new FormatException("boom");
    }

    /// <summary>
    /// Final handler that honours cancellation: throws <see cref="OperationCanceledException"/> if the
    /// token is already cancelled before processing begins.
    /// </summary>
    private class CancelObservingFinalHandler : IFinalRequestHandler<Base, One>, ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="CancelObservingFinalHandler"/> class.</summary>
        /// <param name="logger">Logger used for tracing.</param>
        public CancelObservingFinalHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Throws <see cref="OperationCanceledException"/> when the cancellation token is signalled,
        /// otherwise maps <see cref="Base"/> to <see cref="One"/>.
        /// </summary>
        /// <param name="request">The base request.</param>
        /// <param name="ct">A cancellation token to observe.</param>
        /// <returns>A task that resolves to the mapped <see cref="One"/> response.</returns>
        public Task<One> HandleAsync(Base request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(MapBaseToOne(request));
        }
    }

    /// <summary>
    /// Pipe handler that honours cancellation: throws <see cref="OperationCanceledException"/> before
    /// delegating to the next handler when the token is already cancelled. Used to verify that
    /// <see cref="System.Reflection.BindingFlags.DoNotWrapExceptions"/> applies on the pipe-handler
    /// (hasNext=true) branch of <see cref="Internal.ChainExecutor"/>.
    /// </summary>
    private class CancelObservingPipeHandler : IPipeRequestHandler<Base, Base, One, One>, ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="CancelObservingPipeHandler"/> class.</summary>
        /// <param name="logger">Logger used for tracing.</param>
        public CancelObservingPipeHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Throws <see cref="OperationCanceledException"/> when the cancellation token is signalled,
        /// otherwise delegates to the next handler in the pipeline.
        /// </summary>
        /// <param name="request">The base request.</param>
        /// <param name="ct">A cancellation token to observe.</param>
        /// <param name="next">The next handler delegate in the pipeline.</param>
        /// <returns>A task that resolves to the <see cref="One"/> response.</returns>
        public Task<One> HandleAsync(Base request, CancellationToken ct, Func<Base, CancellationToken, Task<One>> next)
        {
            ct.ThrowIfCancellationRequested();
            return next(request, ct);
        }
    }

    /// <summary>
    /// Pipe handler that passes the request through to the next handler unchanged (same-type pipe).
    /// Used to verify that ChainBuilder picks a same-type pipe as the first chain element and
    /// that it correctly advances to the final handler.
    /// </summary>
    private class PassthroughPipeHandler : IPipeRequestHandler<Base, Base, One, One>, ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="PassthroughPipeHandler"/> class.</summary>
        /// <param name="logger">Logger used for tracing.</param>
        public PassthroughPipeHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Delegates the request to the next handler in the pipeline without modification.
        /// </summary>
        /// <param name="request">The base request.</param>
        /// <param name="ct">A cancellation token to observe.</param>
        /// <param name="next">The next handler delegate in the pipeline.</param>
        /// <returns>A task that resolves to the <see cref="One"/> response from the next handler.</returns>
        public Task<One> HandleAsync(
            Base request,
            CancellationToken ct,
            Func<Base, CancellationToken, Task<One>> next
        ) => next(request, ct);
    }

    /// <summary>
    /// Singleton collector that records the per-instance <see cref="Guid"/> of every
    /// <see cref="ScopeProbeHandler"/> that handled a request.
    /// </summary>
    private sealed class InstanceCollector
    {
        /// <summary>Gets the ordered list of instance identifiers captured during test execution.</summary>
        public System.Collections.Generic.List<Guid> Ids { get; } = new();
    }

    /// <summary>
    /// Scoped final handler that records its own instance identifier in <see cref="InstanceCollector"/>
    /// on every invocation. Because the handler is Scoped, a fresh instance (and thus a fresh
    /// <see cref="Guid"/>) is created for each <c>SendAsync</c> call that opens a new scope.
    /// </summary>
    private class ScopeProbeHandler : IFinalRequestHandler<Base, One>, ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>Unique identifier assigned when this instance is created.</summary>
        private readonly Guid _id = Guid.NewGuid();

        /// <summary>The singleton collector shared across all handler instances.</summary>
        private readonly InstanceCollector _collector;

        /// <summary>Initializes a new instance of the <see cref="ScopeProbeHandler"/> class.</summary>
        /// <param name="collector">Collector recording each resolved handler instance.</param>
        /// <param name="logger">Logger used for tracing.</param>
        public ScopeProbeHandler(InstanceCollector collector, ILogger logger)
        {
            _collector = collector;
            Logger = logger;
        }

        /// <summary>
        /// Records <see cref="_id"/> in <see cref="_collector"/> and maps <see cref="Base"/> to <see cref="One"/>.
        /// </summary>
        /// <param name="request">The base request.</param>
        /// <param name="ct">A cancellation token (unused but required by the interface).</param>
        /// <returns>A task that resolves to the mapped <see cref="One"/> response.</returns>
        public Task<One> HandleAsync(Base request, CancellationToken ct)
        {
            _collector.Ids.Add(_id);
            return Task.FromResult(MapBaseToOne(request));
        }
    }

    /// <summary>Maps a <see cref="Base"/> request to a <see cref="One"/> response (shared by the final test handlers).</summary>
    /// <param name="request">The base request whose value is mapped.</param>
    /// <returns>A <see cref="One"/> whose <see cref="One.First"/> is the value length and whose value is the request value.</returns>
    private static One MapBaseToOne(Base request) =>
        new() { First = request.Value.NotNull().Length, Value = request.Value };

    /// <summary>Base class for test requests and responses.</summary>
    private class Base
    {
        /// <summary>Gets or sets the value.</summary>
        public string? Value { get; init; }

        /// <summary>
        /// Returns a hash code derived from the <see cref="Value"/> property.
        /// </summary>
        /// <returns>An integer hash code for this instance.</returns>
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    }

    /// <summary>Derived class representing a response with a First property.</summary>
    private class One : Base
    {
        /// <summary>Gets or sets the first value.</summary>
        public long First { get; init; }

        /// <summary>
        /// Returns a hash code derived from the base <see cref="Base.Value"/> and the <see cref="First"/> property.
        /// </summary>
        /// <returns>An integer hash code for this instance.</returns>
        public override int GetHashCode() => 7 * base.GetHashCode() + First.GetHashCode();
    }

    /// <summary>Derived class representing a request with a Second property.</summary>
    private class Two : Base
    {
        /// <summary>Gets or sets the second value.</summary>
        public int Second { get; init; }

        /// <summary>
        /// Returns a hash code derived from the base <see cref="Base.Value"/> and the <see cref="Second"/> property.
        /// </summary>
        /// <returns>An integer hash code for this instance.</returns>
        public override int GetHashCode() => 11 * base.GetHashCode() + Second.GetHashCode();
    }
}
