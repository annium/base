using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Linq;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Graylog.Logging.Internal;

/// <summary>
/// Buffering log handler that asynchronously transmits log messages to Graylog using GELF format over HTTP.
/// </summary>
/// <typeparam name="TContext">The type of the logging context that provides additional contextual information for log messages.</typeparam>
internal class GraylogLogHandler<TContext> : BufferingLogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// Function that converts log messages to GELF format dictionaries for transmission.
    /// </summary>
    private readonly Func<LogMessage<TContext>, IReadOnlyDictionary<string, object?>> _format;
    
    /// <summary>
    /// HTTP request factory used to create requests for sending GELF messages to Graylog.
    /// </summary>
    private readonly IHttpRequestFactory _httpRequestFactory;
    
    /// <summary>
    /// JSON serializer used to convert GELF dictionaries to JSON strings for HTTP transmission.
    /// </summary>
    private readonly ISerializer<string> _serializer;
    
    /// <summary>
    /// Configuration settings containing Graylog endpoint and project information.
    /// </summary>
    private readonly GraylogConfiguration _cfg;

    /// <summary>
    /// Initializes a new instance of the GraylogLogHandler class with the specified dependencies and configuration.
    /// </summary>
    /// <param name="httpRequestFactory">The HTTP request factory for creating requests to the Graylog endpoint.</param>
    /// <param name="serializer">The JSON serializer for converting GELF messages to string format.</param>
    /// <param name="cfg">The Graylog configuration containing endpoint and project settings.</param>
    public GraylogLogHandler(
        IHttpRequestFactory httpRequestFactory,
        ISerializer<string> serializer,
        GraylogConfiguration cfg
    )
        : base(cfg)
    {
        _format = Gelf<TContext>.CreateFormat(cfg.Project);
        _httpRequestFactory = httpRequestFactory;
        _serializer = serializer;
        _cfg = cfg;
    }

    /// <summary>
    /// Asynchronously sends a collection of log events to Graylog by converting them to GELF format and transmitting via HTTP POST.
    /// </summary>
    /// <param name="events">The collection of log messages to be sent to Graylog.</param>
    /// <returns>A task that represents the asynchronous send operation, returning true if successful, false otherwise.</returns>
    protected override async ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<TContext>> events)
    {
        var data = events.Select(x => _serializer.Serialize(_format(x))).Join(Environment.NewLine);

        try
        {
            var response = await _httpRequestFactory.New(_cfg.Endpoint).Post("gelf").StringContent(data).RunAsync();
            if (response.IsSuccess)
                return true;

            Console.WriteLine(
                $"Failed to to write events to Graylog at {_cfg.Endpoint}: {response.StatusCode} - {response.StatusText}"
            );
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to to write events to Graylog at {_cfg.Endpoint}: {e}");
            return false;
        }
    }
}
