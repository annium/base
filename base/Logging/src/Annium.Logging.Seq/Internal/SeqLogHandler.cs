using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Linq;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Logging.Seq.Internal;

internal class SeqLogHandler<TContext> : BufferingLogHandler<TContext>
    where TContext : class, ILogContext
{
    private readonly Func<LogMessage<TContext>, IReadOnlyDictionary<string, string?>> _format;
    private readonly IHttpRequestFactory _httpRequestFactory;
    private readonly ISerializer<string> _serializer;
    private readonly SeqConfiguration _cfg;

    public SeqLogHandler(
        IHttpRequestFactory httpRequestFactory,
        ISerializer<string> serializer,
        SeqConfiguration cfg
    ) : base(
        cfg
    )
    {
        _format = CompactLogEvent<TContext>.CreateFormat(cfg.Project);
        _httpRequestFactory = httpRequestFactory;
        _serializer = serializer;
        _cfg = cfg;
    }

    protected override async ValueTask<bool> SendEventsAsync(
        IReadOnlyCollection<LogMessage<TContext>> events
    )
    {
        var data = events.Select(x => _serializer.Serialize(_format(x))).Join(Environment.NewLine);

        try
        {
            var response = await _httpRequestFactory.New(_cfg.Endpoint)
                .Post("api/events/raw")
                .Param("clef", string.Empty)
                .Header("X-Seq-ApiKey", _cfg.ApiKey)
                .StringContent(data, "application/vnd.serilog.clef")
                .RunAsync();
            if (response.IsSuccess)
                return true;

            Console.WriteLine($"Failed to to write events to Seq at {_cfg.Endpoint}: {response.StatusCode} - {response.StatusText}");
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to to write events to Seq at {_cfg.Endpoint}: {e}");
            return false;
        }
    }
}