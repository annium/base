using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="XunitLogHandler{TContext}"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit output helper the messages are written to.</param>
    /// <param name="format">Formatter turning a log message into the output line.</param>
    public XunitLogHandler(ITestOutputHelper outputHelper, Func<LogMessage<TContext>, string> format)
    {
        _outputHelper = outputHelper;
        _format = format;
    }

    /// <summary>
    /// Writes each message in the batch to the xUnit test output. Failures (e.g., output disposed
    /// after the test ends) are swallowed because xUnit otherwise treats them as fatal.
    /// </summary>
    /// <param name="messages">The log messages to write</param>
    /// <param name="ct">Cancellation token (unused — output writes are synchronous)</param>
    /// <returns>A completed value task</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<TContext>> messages, CancellationToken ct)
    {
        foreach (var msg in messages)
        {
            try
            {
                _outputHelper.WriteLine(_format(msg));
            }
            catch
            {
                // ignored — test output may be unavailable after the test completes
            }
        }

        return ValueTask.CompletedTask;
    }
}
