using System;
using Annium.Logging.Shared;
using Xunit;

namespace Annium.Logging.Xunit.Internal;

/// <summary>
/// Log handler that writes log messages to xUnit test output
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
internal class XunitLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// The xUnit test output helper for writing log messages
    /// </summary>
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// The format function for converting log messages to strings
    /// </summary>
    private readonly Func<LogMessage<TContext>, string> _format;

    public XunitLogHandler(ITestOutputHelper outputHelper, Func<LogMessage<TContext>, string> format)
    {
        _outputHelper = outputHelper;
        _format = format;
    }

    /// <summary>
    /// Handles a log message by writing it to the test output
    /// </summary>
    /// <param name="msg">The log message to handle</param>
    public void Handle(LogMessage<TContext> msg)
    {
        try
        {
            _outputHelper.WriteLine(_format(msg));
        }
        catch
        {
            // ignored
        }
    }
}
