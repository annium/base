using System;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Console.Tests;

public class LoggerTests : TestBase
{
    public LoggerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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
