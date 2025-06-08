using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Shared;

namespace Annium.Logging.File.Internal;

/// <summary>
/// Log handler that writes log messages to files with buffering and grouping support.
/// Supports writing to multiple files based on message content and provides thread-safe file operations.
/// </summary>
/// <typeparam name="TContext">The type of the log context</typeparam>
internal class FileLogHandler<TContext> : BufferingLogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// Synchronization gate for thread-safe file operations.
    /// </summary>
    private readonly AutoResetEvent _gate = new(true);

    /// <summary>
    /// Function to format log messages for file output.
    /// </summary>
    private readonly Func<LogMessage<TContext>, string> _format;

    /// <summary>
    /// Configuration for file logging including file path resolution.
    /// </summary>
    private readonly FileLoggingConfiguration<TContext> _cfg;

    /// <summary>
    /// Cache of files that have been initialized to avoid repeated initialization.
    /// </summary>
    private readonly ConcurrentDictionary<string, bool> _files = new();

    public FileLogHandler(Func<LogMessage<TContext>, string> format, FileLoggingConfiguration<TContext> cfg)
        : base(cfg)
    {
        _format = format;
        _cfg = cfg;
    }

    /// <summary>
    /// Sends buffered log events to files, grouped by target file path.
    /// </summary>
    /// <param name="events">The collection of log events to write</param>
    /// <returns>True if all events were written successfully, false otherwise</returns>
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

    /// <summary>
    /// Writes a collection of log events to a specific file.
    /// </summary>
    /// <param name="file">The target file path</param>
    /// <param name="events">The log events to write</param>
    /// <returns>True if events were written successfully, false otherwise</returns>
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
