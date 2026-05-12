using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.ViewModel.Internal.PipeHandlers.Response;
using Annium.Core.Mapper;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.ViewModel.Tests;

/// <summary>
/// Verifies the response-side single mapping pipe handler skips mapping on non-Ok statuses
/// (guards against a null dereference when upstream returns default(TResponseIn)).
/// </summary>
public class MappingSinglePipeHandlerTests
{
    /// <summary>On a non-Ok upstream status the mapper MUST NOT be invoked.</summary>
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

    /// <summary>Test request marker.</summary>
    public class TestRequest { }

    /// <summary>Source DTO returned by upstream.</summary>
    public class TestSource { }

    /// <summary>View-model target the handler maps into.</summary>
    public class TestTarget : IResponse<TestSource> { }

    /// <summary>Mapper that records invocations without producing a result. Used to prove that
    /// non-Ok paths bypass the mapper entirely via the independent <c>Invocations.Is(0)</c>
    /// assertion (rather than relying on a thrown exception as the regression signal).</summary>
    private sealed class RecordingMapper : IMapper
    {
        public int Invocations { get; private set; }

        public bool HasMap<T>(object? source) => true;

        public bool HasMap(object? source, Type? type) => true;

        public T Map<T>(object? source)
        {
            Invocations++;
            return default!;
        }

        public object? Map(object? source, Type type)
        {
            Invocations++;
            return null;
        }
    }

    /// <summary>Mapper that returns a fixed instance — used to prove the Ok path does invoke mapping.</summary>
    private sealed class StubMapper : IMapper
    {
        private readonly object _result;

        public StubMapper(object result)
        {
            _result = result;
        }

        public int Invocations { get; private set; }

        public bool HasMap<T>(object? source) => true;

        public bool HasMap(object? source, Type? type) => true;

        public T Map<T>(object? source)
        {
            Invocations++;
            return (T)_result;
        }

        public object? Map(object? source, Type type)
        {
            Invocations++;
            return _result;
        }
    }

    /// <summary>No-op logger sufficient for these unit tests.</summary>
    private sealed class NullLogger : ILogger
    {
        public void Log(
            object subject,
            string file,
            string member,
            int line,
            LogLevel level,
            string message,
            IReadOnlyList<object?> data
        ) { }

        public void Error(
            object subject,
            string file,
            string member,
            int line,
            Exception ex,
            IReadOnlyList<object?> data
        ) { }
    }
}
