using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Linq;
using Annium.Logging.Shared;
using Annium.Testing;
using Annium.Testing.Collection;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Logging.File.Tests;

/// <summary>
/// Tests for file logging functionality
/// </summary>
public class LoggerTests : TestBase
{
    /// <summary>
    /// Temporary file path for testing log output
    /// </summary>
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
            sp.UseLogging(route =>
                route.UseFile(
                    new FileLoggingConfiguration<DefaultLogContext>
                    {
                        BufferTime = TimeSpan.Zero,
                        BufferCount = 1,
                        GetFile = _ => _logFile,
                    }
                )
            );
        });
    }

    /// <summary>
    /// Tests that log messages are written to file
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task LogMessage_WritesLogMessage()
    {
        // arrange
        var subject = GetSubject();

        // act
        subject.Info("one");
        subject.Info("two");

        // assert
        await Wait.UntilAsync(() => GetLog().Count == 2);
        var log = GetLog();
        log.At(0).Contains("one").IsTrue();
        log.At(1).Contains("two").IsTrue();
    }

    /// <summary>
    /// Tests that aggregate exceptions write error count and all errors to file
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task LogAggregateException_WritesErrorsCountAndAllErrors()
    {
        // arrange
        var subject = GetSubject();

        // arrange
        var ex = new AggregateException(new Exception("xxx"), new Exception("yyy"));

        // act
        subject.Error(ex);

        // assert
        await Wait.UntilAsync(() => GetLog().Any());
        var log = GetLog().Join(Environment.NewLine);
        log.Contains("2 error(s) in").IsTrue();
        log.Contains("xxx").IsTrue();
        log.Contains("yyy").IsTrue();
    }

    /// <summary>
    /// Tests that exceptions are written to file
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task LogException_WritesException()
    {
        // arrange
        var subject = GetSubject();

        // arrange
        var ex = new Exception("xxx");

        // act
        subject.Error(ex);

        // assert
        await Wait.UntilAsync(() => GetLog().Any());
        var log = GetLog().Join(Environment.NewLine);
        log.Contains("xxx").IsTrue();
    }

    /// <summary>
    /// Creates a test subject with file logging configured
    /// </summary>
    /// <returns>A log subject for testing</returns>
    private ILogSubject GetSubject() => Get<ILogBridgeFactory>().Get("test");

    /// <summary>
    /// Reads the log file contents as lines
    /// </summary>
    /// <returns>The log file lines</returns>
    private IReadOnlyList<string> GetLog() => System.IO.File.ReadAllLines(_logFile);
}
