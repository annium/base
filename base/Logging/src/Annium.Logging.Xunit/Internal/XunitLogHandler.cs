using System;
using Annium.Logging.Shared;
using Xunit.Abstractions;

namespace Annium.Logging.Xunit.Internal;

internal class XunitLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly Func<LogMessage<TContext>, string> _format;

    public XunitLogHandler(ITestOutputHelper outputHelper, Func<LogMessage<TContext>, string> format)
    {
        _outputHelper = outputHelper;
        _format = format;
    }

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
