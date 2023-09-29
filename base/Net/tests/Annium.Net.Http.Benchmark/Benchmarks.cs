using System;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Net.Http.Benchmark.Internal;
using BenchmarkDotNet.Attributes;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.Http.Benchmark;

[MemoryDiagnoser]
public class Benchmarks
{
    [Params(1000)]
    public int TotalRequests { get; set; }

    [Params(10, 100, 1000)]
    public int Size { get; set; }

    private static readonly Uri ServerUri = new($"http://127.0.0.1:{Constants.Port}/");
    private readonly IHttpRequestFactory _httpRequestFactory;

    public Benchmarks()
    {
        var container = new ServiceContainer();
        container.AddHttpRequestFactory(true);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();

        var sp = container.BuildServiceProvider();
        sp.UseLogging(route => route.UseInMemory());

        _httpRequestFactory = sp.Resolve<IHttpRequestFactory>();
    }

    [Benchmark]
    public async Task Params()
    {
        for (var i = 0; i < TotalRequests; i++)
        {
            var request = _httpRequestFactory.New(ServerUri).Get("/params");
            for (var j = 0; j < Size; j++)
                request.Param($"x{j}", j);
            var response = await request.RunAsync();
            if (response.IsFailure)
                throw new Exception($"Response #{i} failed");
        }
    }

    [Benchmark]
    public async Task Upload()
    {
        for (var i = 0; i < TotalRequests; i++)
        {
            var request = _httpRequestFactory.New(ServerUri).Get("/upload").Attach(new ByteArrayContent(Helper.GetContent(Size)));
            var response = await request.RunAsync();
            if (response.IsFailure)
                throw new Exception($"Response #{i} failed");
        }
    }

    [Benchmark]
    public async Task Download()
    {
        for (var i = 0; i < TotalRequests; i++)
        {
            var request = _httpRequestFactory.New(ServerUri).Get("/download").Param("size", Size);
            var response = await request.RunAsync();
            if (response.IsFailure)
                throw new Exception($"Response #{i} failed");
        }
    }
}