using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Shared;

namespace Annium.Logging.File.Internal;

internal class FileLogHandler<TContext> : BufferingLogHandler<TContext>
    where TContext : class, ILogContext
{
    private readonly AutoResetEvent _gate = new(true);
    private readonly Func<LogMessage<TContext>, string> _format;
    private readonly FileLoggingConfiguration<TContext> _cfg;
    private readonly ConcurrentDictionary<string, bool> _files = new();

    public FileLogHandler(
        Func<LogMessage<TContext>, string> format,
        FileLoggingConfiguration<TContext> cfg
    ) : base(cfg)
    {
        _format = format;
        _cfg = cfg;
    }

    protected override async ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<TContext>> events)
    {
        if (_cfg.GetFile is null)
            throw new InvalidOperationException("GetFile resolver is not set");

        try
        {
            _gate.WaitOne();
            var entries = events.GroupBy(_cfg.GetFile);
            var results = await Task.WhenAll(entries.Select(x => WriteEventsToFileAsync(x.Key, x.ToArray())));

            return results.All(x => x);
        }
        finally
        {
            _gate.Set();
        }
    }

    private async Task<bool> WriteEventsToFileAsync(string file, IReadOnlyCollection<LogMessage<TContext>> events)
    {
        try
        {
            if (_files.TryAdd(file, true))
                await System.IO.File.WriteAllTextAsync(file, string.Empty);

            await System.IO.File.AppendAllLinesAsync(file, events.Select(_format));
            await System.IO.File.AppendAllTextAsync(file, string.Empty);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to to write events to {file}: {e}");
            return false;
        }
    }
}