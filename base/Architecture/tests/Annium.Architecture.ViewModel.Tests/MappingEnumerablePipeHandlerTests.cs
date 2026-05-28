using System;
using System.Collections.Generic;
using System.Linq;
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
/// Verifies the response-side enumerable mapping pipe handler skips mapping on non-Ok statuses
/// (guards against a null enumerable dereference when upstream returns default(IEnumerable&lt;TResponseIn&gt;)).
/// </summary>
public class MappingEnumerablePipeHandlerTests
{
    /// <summary>On a non-Ok upstream status the mapper MUST NOT be invoked.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_NonOkStatus_DoesNotInvokeMapper()
    {
        // arrange
        var mapper = new RecordingMapper();
        var handler = new MappingEnumerablePipeHandler<TestRequest, TestSource, TestTarget>(mapper, new NullLogger());

        // act
        var result = await handler.HandleAsync(
            new TestRequest(),
            CancellationToken.None,
            (_, _) =>
                Task.FromResult<IStatusResult<OperationStatus, IEnumerable<TestSource>>>(
                    Result.Status(OperationStatus.Forbidden, default(IEnumerable<TestSource>)!).Error("denied")
                )
        );

        // assert: status and errors propagate, an empty enumerable is returned, mapper skipped
        result.Status.Is(OperationStatus.Forbidden);
        result.PlainErrors.Has(1);
        result.Data.Count().Is(0);
        mapper.Invocations.Is(0);
    }

    /// <summary>On Ok the mapper IS invoked and the mapped enumerable is returned.</summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_OkStatus_InvokesMapper()
    {
        // arrange
        IEnumerable<TestTarget> mapped = new[] { new TestTarget(), new TestTarget() };
        var mapper = new StubMapper(mapped);
        var handler = new MappingEnumerablePipeHandler<TestRequest, TestSource, TestTarget>(mapper, new NullLogger());

        // act
        var result = await handler.HandleAsync(
            new TestRequest(),
            CancellationToken.None,
            (_, _) =>
                Task.FromResult<IStatusResult<OperationStatus, IEnumerable<TestSource>>>(
                    Result.Status(OperationStatus.Ok, (IEnumerable<TestSource>)new[] { new TestSource() })
                )
        );

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.Data.Count().Is(2);
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
        var handler = new MappingEnumerablePipeHandler<TestRequest, TestSource, TestTarget>(mapper, new NullLogger());

        // act + assert
        await Wrap.It(async () =>
                await handler.HandleAsync(
                    new TestRequest(),
                    CancellationToken.None,
                    (_, _) =>
                        Task.FromResult<IStatusResult<OperationStatus, IEnumerable<TestSource>>>(
                            Result.Status(OperationStatus.Ok, default(IEnumerable<TestSource>)!)
                        )
                )
            )
            .ThrowsAsync<InvalidOperationException>();

        mapper.Invocations.Is(0);
    }
}
