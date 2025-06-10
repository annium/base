using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Linq;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Net.Http.Extensions;
using Annium.Serialization.Abstractions;

namespace Annium.Seq.Logging.Internal;

/// <summary>
/// Log handler implementation that sends buffered log events to a Seq server using HTTP requests.
/// Formats log messages as Compact Log Event Format (CLEF) and transmits them via Seq's raw events API.
/// </summary>
/// <typeparam name="TContext">The type of the logging context containing additional properties</typeparam>
internal class SeqLogHandler<TContext> : BufferingLogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// The CLEF formatter function that converts log messages to dictionary format for Seq ingestion.
    /// </summary>
    private readonly Func<LogMessage<TContext>, IReadOnlyDictionary<string, string?>> _format;

    /// <summary>
    /// Factory for creating HTTP requests to send log events to the Seq server.
    /// </summary>
    private readonly IHttpRequestFactory _httpRequestFactory;

    /// <summary>
    /// JSON serializer for converting CLEF dictionaries to string format for HTTP transmission.
    /// </summary>
    private readonly ISerializer<string> _serializer;

    /// <summary>
    /// Configuration settings for the Seq server connection including endpoint and API key.
    /// </summary>
    private readonly SeqConfiguration _cfg;

    /// <summary>
    /// Initializes a new instance of the SeqLogHandler with the required dependencies.
    /// </summary>
    /// <param name="httpRequestFactory">Factory for creating HTTP requests to the Seq server</param>
    /// <param name="serializer">JSON serializer for formatting log data</param>
    /// <param name="cfg">Seq server configuration including endpoint, API key, and project settings</param>
    public SeqLogHandler(IHttpRequestFactory httpRequestFactory, ISerializer<string> serializer, SeqConfiguration cfg)
        : base(cfg)
    {
        _format = CompactLogEvent<TContext>.CreateFormat(cfg.Project);
        _httpRequestFactory = httpRequestFactory;
        _serializer = serializer;
        _cfg = cfg;
    }

    /// <summary>
    /// Sends a collection of log events to the Seq server using the raw events API.
    /// Formats events as CLEF and transmits them via HTTP POST with proper content type and authentication.
    /// </summary>
    /// <param name="events">The collection of log messages to send to Seq</param>
    /// <returns>True if all events were successfully sent; false if the request failed</returns>
    protected override async ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<TContext>> events)
    {
        var data = events.Select(x => _serializer.Serialize(_format(x))).Join(Environment.NewLine);

        try
        {
            var response = await _httpRequestFactory
                .New(_cfg.Endpoint)
                .Post("api/events/raw")
                .Param("clef", string.Empty)
                .Header("X-Seq-ApiKey", _cfg.ApiKey)
                .StringContent(data, "application/vnd.serilog.clef")
                .RunAsync();
            if (response.IsSuccess)
                return true;

            Console.WriteLine(
                $"Failed to to write events to Seq at {_cfg.Endpoint}: {response.StatusCode} - {response.StatusText}"
            );
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to to write events to Seq at {_cfg.Endpoint}: {e}");
            return false;
        }
    }
}
