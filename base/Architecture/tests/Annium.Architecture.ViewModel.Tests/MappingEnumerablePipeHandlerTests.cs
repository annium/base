using System;
using System.Collections.Generic;
using System.Linq;
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
/// Verifies the response-side enumerable mapping pipe handler skips mapping on non-Ok statuses
/// (guards against a null enumerable dereference when upstream returns default(IEnumerable&lt;TResponseIn&gt;)).
/// </summary>
public class MappingEnumerablePipeHandlerTests
{
    /// <summary>On a non-Ok upstream status the mapper MUST NOT be invoked.</summary>
    [Fact]
    public async Task HandleAsync_NonOkStatus_DoesNotInvokeMapper()
    {
        // arrange
        var mapper = new ThrowingMapper();
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

    /// <summary>Test request marker.</summary>
    public class TestRequest { }

    /// <summary>Source DTO returned by upstream.</summary>
    public class TestSource { }

    /// <summary>View-model target the handler maps into.</summary>
    public class TestTarget : IResponse<TestSource> { }

    /// <summary>Mapper that throws on every Map call — used to prove the non-Ok path skips mapping.</summary>
    private sealed class ThrowingMapper : IMapper
    {
        public int Invocations { get; private set; }

        public bool HasMap<T>(object? source) => true;

        public bool HasMap(object? source, Type? type) => true;

        public T Map<T>(object? source)
        {
            Invocations++;
            throw new InvalidOperationException("Mapper should not be invoked on non-Ok responses");
        }

        public object? Map(object? source, Type type)
        {
            Invocations++;
            throw new InvalidOperationException("Mapper should not be invoked on non-Ok responses");
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
