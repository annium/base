using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Linq;
using Annium.Logging.Shared;
using Annium.Testing;
using Annium.Threading.Tasks;
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

    private ILogSubject GetSubject() => Get<ILogBridgeFactory>().Get("test");

    private IReadOnlyList<string> GetLog() => System.IO.File.ReadAllLines(_logFile);
}
