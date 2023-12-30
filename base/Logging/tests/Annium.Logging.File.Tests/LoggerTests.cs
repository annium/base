using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Logging.File.Tests;

public class LoggerTests : TestBase
{
    private readonly string _logFile = Path.GetTempFileName();

    public LoggerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddLogging();
        });

        Setup(sp =>
        {
            sp.UseLogging(
                route =>
                    route.UseFile(
                        new FileLoggingConfiguration<DefaultLogContext>
                        {
                            BufferTime = TimeSpan.Zero,
                            BufferCount = 1,
                            GetFile = _ => _logFile
                        }
                    )
            );
        });
    }

    [Fact]
    public void LogMessage_WritesLogMessage()
    {
        // arrange
        var subject = GetSubject();

        // act
        subject.Info("sample");

        // assert
        var log = GetLog();
        log.Contains("sample").IsTrue();
    }

    [Fact]
    public void LogAggregateException_WritesErrorsCountAndAllErrors()
    {
        // arrange
        var subject = GetSubject();

        // arrange
        var ex = new AggregateException(new Exception("xxx"), new Exception("yyy"));

        // act
        subject.Error(ex);

        // assert
        var log = GetLog();
        log.Contains("2 error(s) in").IsTrue();
        log.Contains("xxx").IsTrue();
        log.Contains("yyy").IsTrue();
    }

    [Fact]
    public void LogException_WritesException()
    {
        // arrange
        var subject = GetSubject();

        // arrange
        var ex = new Exception("xxx");

        // act
        subject.Error(ex);

        // assert
        var log = GetLog();
        log.Contains("xxx").IsTrue();
    }

    private ILogSubject GetSubject() => Get<ILogSubject>();

    private string GetLog() => System.IO.File.ReadAllText(_logFile);
}
