using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.ViewModel.Internal.PipeHandlers.Response;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;
using static Annium.Architecture.ViewModel.Tests.ResponseMappingFixtures;

namespace Annium.Architecture.ViewModel.Tests;

/// <summary>
/// Verifies the response-side single mapping pipe handler skips mapping on non-Ok statuses
/// (guards against a null dereference when upstream returns default(TResponseIn)).
/// </summary>
public class MappingSinglePipeHandlerTests
{
    /// <summary>On a non-Ok upstream status the mapper MUST NOT be invoked.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_NonOkStatus_DoesNotInvokeMapper()
    {
        // arrange
        var mapper = new RecordingMapper();
        var handler = new MappingSinglePipeHandler<TestRequest, TestSource, TestTarget>(mapper, new NullLogger());

        // act
        var result = await handler.HandleAsync(
            new TestRequest(),
            CancellationToken.None,
            (_, _) =>
                Task.FromResult<IStatusResult<OperationStatus, TestSource>>(
                    Result.Status(OperationStatus.NotFound, default(TestSource)!).Error("missing")
                )
        );

        // assert: status and errors propagate; mapping is bypassed
        result.Status.Is(OperationStatus.NotFound);
        result.PlainErrors.Has(1);
        mapper.Invocations.Is(0);
    }

    /// <summary>On Ok the mapper IS invoked and the mapped value is returned.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_OkStatus_InvokesMapper()
    {
        // arrange
        var target = new TestTarget();
        var mapper = new StubMapper(target);
        var handler = new MappingSinglePipeHandler<TestRequest, TestSource, TestTarget>(mapper, new NullLogger());

        // act
        var result = await handler.HandleAsync(
            new TestRequest(),
            CancellationToken.None,
            (_, _) =>
                Task.FromResult<IStatusResult<OperationStatus, TestSource>>(
                    Result.Status(OperationStatus.Ok, new TestSource())
                )
        );

        // assert
        result.Status.Is(OperationStatus.Ok);
        ReferenceEquals(result.Data, target).IsTrue();
        mapper.Invocations.Is(1);
    }

    /// <summary>Status=Ok with Data=null is a contract violation; the handler MUST throw so the
    /// upstream ExceptionPipeHandler can convert it to UncaughtError instead of silently mapping null.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_OkStatusWithNullData_Throws()
    {
        // arrange
        var mapper = new RecordingMapper();
        var handler = new MappingSinglePipeHandler<TestRequest, TestSource, TestTarget>(mapper, new NullLogger());

        // act + assert
        await Wrap.It(async () =>
                await handler.HandleAsync(
                    new TestRequest(),
                    CancellationToken.None,
                    (_, _) =>
                        Task.FromResult<IStatusResult<OperationStatus, TestSource>>(
                            Result.Status(OperationStatus.Ok, default(TestSource)!)
                        )
                )
            )
            .ThrowsAsync<InvalidOperationException>();

        mapper.Invocations.Is(0);
    }
}
