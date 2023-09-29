using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared.Internal;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Logging.Shared.Tests;

public class BaseLoggerTest : TestBase
{
    private readonly IList<LogMessage<Context>> _messages = new List<LogMessage<Context>>();

    public BaseLoggerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }


    [Fact]
    public void Log_ValidLevel_WritesLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject>();
        var timeProvider = provider.Resolve<ITimeProvider>();

        // act
        subject.Info("sample");

        // assert
        _messages.Has(1);
        _messages.At(0).Instant.Is(timeProvider.Now);
        _messages.At(0).Level.Is(LogLevel.Info);
        _messages.At(0).SubjectType.Is(typeof(LogSubject).FriendlyName());
        _messages.At(0).SubjectId.IsNullOrWhiteSpace().IsFalse();
        _messages.At(0).Message.Is("sample");
    }

    [Fact]
    public void Log_InvalidLevel_OmitsLogEntry()
    {
        // arrange
        var provider = GetProvider(LogLevel.Warn);
        var subject = provider.Resolve<ILogSubject>();

        // act
        subject.Info("sample");

        // assert
        _messages.IsEmpty();
    }

    [Fact]
    public void LogTrace_WritesTraceLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject>();

        // act
        subject.Info("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Info);
    }

    [Fact]
    public void LogDebug_WritesDebugLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject>();

        // act
        subject.Warn("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Warn);
    }

    [Fact]
    public void LogInfo_WritesInfoLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject>();

        // act
        subject.Info("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Info);
    }

    [Fact]
    public void LogWarn_WritesWarnLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject>();

        // act
        subject.Warn("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Warn);
    }

    [Fact]
    public void LogErrorException_WritesErrorExceptionLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject>();
        var exception = new Exception("sample");

        // act
        subject.Error(exception);

        // assert
        _messages.At(0).Level.Is(LogLevel.Error);
        _messages.At(0).Message.Is(exception.Message);
        _messages.At(0).Exception.Is(exception);
    }

    [Fact]
    public void LogErrorMessage_WritesErrorMessageLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject>();

        // act
        subject.Error("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Error);
        _messages.At(0).Message.Is("sample");
    }

    private IServiceProvider GetProvider(LogLevel minLogLevel = LogLevel.Trace)
    {
        var container = new ServiceContainer();

        container.AddTime().WithManagedTime().SetDefault();

        container.AddLogging<Context>();

        var provider = container.BuildServiceProvider();

        provider.UseLogging<Context>(route => route
            .For(m => m.Level >= minLogLevel)
            .UseInstance(new LogHandler(_messages), new LogRouteConfiguration())
        );

        return provider;
    }

    private class LogHandler : ILogHandler<Context>
    {
        public IList<LogMessage<Context>> Messages { get; }

        public LogHandler(
            IList<LogMessage<Context>> messages
        )
        {
            Messages = messages;
        }

        public void Handle(LogMessage<Context> message)
        {
            Messages.Add(message);
        }
    }

    private class Context : ILogContext
    {
    }
}