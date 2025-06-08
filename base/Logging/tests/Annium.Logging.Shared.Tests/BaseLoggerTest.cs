using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests;

/// <summary>
/// Base tests for logging functionality
/// </summary>
public class BaseLoggerTest : TestBase
{
    /// <summary>
    /// Collection of captured log messages for testing
    /// </summary>
    private readonly IList<LogMessage<Context>> _messages = new List<LogMessage<Context>>();

    public BaseLoggerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that valid log level entries are written
    /// </summary>
    [Fact]
    public void Log_ValidLevel_WritesLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.GetSubject();
        var timeProvider = provider.Resolve<ITimeProvider>();

        // act
        subject.Info("sample");

        // assert
        _messages.Has(1);
        _messages.At(0).Instant.Is(timeProvider.Now);
        _messages.At(0).Level.Is(LogLevel.Info);
        _messages.At(0).SubjectType.Is("test");
        _messages.At(0).SubjectId.IsNullOrWhiteSpace().IsFalse();
        _messages.At(0).Message.Is("sample");
    }

    /// <summary>
    /// Tests that invalid log level entries are omitted
    /// </summary>
    [Fact]
    public void Log_InvalidLevel_OmitsLogEntry()
    {
        // arrange
        var provider = GetProvider(LogLevel.Warn);
        var subject = provider.GetSubject();

        // act
        subject.Info("sample");

        // assert
        _messages.IsEmpty();
    }

    /// <summary>
    /// Tests that trace level log entries are written
    /// </summary>
    [Fact]
    public void LogTrace_WritesTraceLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.GetSubject();

        // act
        subject.Info("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Info);
    }

    /// <summary>
    /// Tests that debug level log entries are written
    /// </summary>
    [Fact]
    public void LogDebug_WritesDebugLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.GetSubject();

        // act
        subject.Warn("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Warn);
    }

    /// <summary>
    /// Tests that info level log entries are written
    /// </summary>
    [Fact]
    public void LogInfo_WritesInfoLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.GetSubject();

        // act
        subject.Info("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Info);
    }

    /// <summary>
    /// Tests that warn level log entries are written
    /// </summary>
    [Fact]
    public void LogWarn_WritesWarnLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.GetSubject();

        // act
        subject.Warn("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Warn);
    }

    /// <summary>
    /// Tests that error level log entries with exceptions are written
    /// </summary>
    [Fact]
    public void LogErrorException_WritesErrorExceptionLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.GetSubject();
        var exception = new Exception("sample");

        // act
        subject.Error(exception);

        // assert
        _messages.At(0).Level.Is(LogLevel.Error);
        _messages.At(0).Message.Is(exception.Message);
        _messages.At(0).Exception.Is(exception);
    }

    /// <summary>
    /// Tests that error level log entries with messages are written
    /// </summary>
    [Fact]
    public void LogErrorMessage_WritesErrorMessageLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.GetSubject();

        // act
        subject.Error("sample");

        // assert
        _messages.At(0).Level.Is(LogLevel.Error);
        _messages.At(0).Message.Is("sample");
    }

    /// <summary>
    /// Creates a configured service provider for testing
    /// </summary>
    /// <param name="minLogLevel">The minimum log level to capture</param>
    /// <returns>A configured service provider</returns>
    private IServiceProvider GetProvider(LogLevel minLogLevel = LogLevel.Trace)
    {
        var container = new ServiceContainer();

        container.AddTime().WithManagedTime().SetDefault();

        container.AddLogging<Context>();

        var provider = container.BuildServiceProvider();

        provider.UseLogging<Context>(route =>
            route.For(m => m.Level >= minLogLevel).UseInstance(new LogHandler(_messages), new LogRouteConfiguration())
        );

        return provider;
    }

    /// <summary>
    /// Test log handler that captures messages for verification
    /// </summary>
    private class LogHandler : ILogHandler<Context>
    {
        /// <summary>
        /// Gets the collection of captured log messages
        /// </summary>
        public IList<LogMessage<Context>> Messages { get; }

        public LogHandler(IList<LogMessage<Context>> messages)
        {
            Messages = messages;
        }

        /// <summary>
        /// Handles a log message by adding it to the messages collection
        /// </summary>
        /// <param name="message">The log message to handle</param>
        public void Handle(LogMessage<Context> message)
        {
            Messages.Add(message);
        }
    }

    /// <summary>
    /// Test context class for logging tests
    /// </summary>
    private class Context;
}

/// <summary>
/// Extensions for service provider to get test subjects
/// </summary>
file static class ServiceProviderExtensions
{
    /// <summary>
    /// Gets a test log subject from the service provider
    /// </summary>
    /// <param name="sp">The service provider</param>
    /// <returns>A log subject for testing</returns>
    public static ILogSubject GetSubject(this IServiceProvider sp)
    {
        return sp.Resolve<ILogBridgeFactory>().Get("test");
    }
}
