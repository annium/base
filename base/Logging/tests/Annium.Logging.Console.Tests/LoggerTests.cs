using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.Runtime;
using Annium.Logging.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Console.Tests;

/// <summary>
/// Tests for console logging functionality
/// </summary>
public class LoggerTests : TestBase
{
    public LoggerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that log messages are written to console output
    /// </summary>
    [Fact]
    public void LogMessage_WritesLogMessageToConsole()
    {
        // arrange
        var subject = GetSubject();
        using var capture = ConsoleCapture.Start();

        // act
        subject.Info("sample");

        // assert
        capture.Output.Contains("sample").IsTrue();
    }

    /// <summary>
    /// Tests that aggregate exceptions write error count and all errors to console
    /// </summary>
    [Fact]
    public void LogAggregateException_WritesErrorsCountAndAllErrorsToConsole()
    {
        // arrange
        var subject = GetSubject();
        using var capture = ConsoleCapture.Start();

        // arrange
        var ex = new AggregateException(new Exception("xxx"), new Exception("yyy"));

        // act
        subject.Error(ex);

        // assert
        capture.Output.Contains("2 error(s) in").IsTrue();
        capture.Output.Contains("xxx").IsTrue();
        capture.Output.Contains("yyy").IsTrue();
    }

    /// <summary>
    /// Tests that exceptions are written to console output
    /// </summary>
    [Fact]
    public void LogException_WritesExceptionToConsole()
    {
        // arrange
        var subject = GetSubject();
        using var capture = ConsoleCapture.Start();

        // arrange
        var ex = new Exception("xxx");

        // act
        subject.Error(ex);

        // assert
        capture.Output.Contains("xxx").IsTrue();
    }

    /// <summary>
    /// Creates a test subject with console logging configured
    /// </summary>
    /// <param name="minLogLevel">The minimum log level to capture</param>
    /// <returns>A log subject for testing</returns>
    private ILogSubject GetSubject(LogLevel minLogLevel = LogLevel.Trace)
    {
        var container = new ServiceContainer();

        container.AddTime().WithManagedTime().SetDefault();

        container.AddLogging();

        var provider = container.BuildServiceProvider();
        provider.UseLogging(route => route.For(m => m.Level >= minLogLevel).UseConsole());

        return provider.Resolve<ILogBridgeFactory>().Get("test");
    }
}
