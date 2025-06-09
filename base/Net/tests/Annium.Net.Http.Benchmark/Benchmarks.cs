using System;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Net.Http.Benchmark.Internal;
using BenchmarkDotNet.Attributes;

namespace Annium.Net.Http.Benchmark;

/// <summary>
/// Benchmark class for HTTP request operations.
/// </summary>
[MemoryDiagnoser]
public class Benchmarks
{
    /// <summary>
    /// Gets or sets the total number of requests to execute in the benchmark.
    /// </summary>
    [Params(1000)]
    public int TotalRequests { get; set; }

    /// <summary>
    /// Gets or sets the size parameter for the benchmark (affects payload size).
    /// </summary>
    [Params(10, 100, 1000)]
    public int Size { get; set; }

    /// <summary>
    /// The server URI for benchmark requests.
    /// </summary>
    private static readonly Uri _serverUri = new($"http://127.0.0.1:{Constants.Port}/");

    /// <summary>
    /// The HTTP request factory for creating requests.
    /// </summary>
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

    /// <summary>
    /// Benchmarks HTTP requests with query parameters.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Benchmark]
    public async Task ParamsAsync()
    {
        for (var i = 0; i < TotalRequests; i++)
        {
            var request = _httpRequestFactory.New(_serverUri).Get("/params");
            for (var j = 0; j < Size; j++)
                request.Param($"x{j}", j);
            var response = await request.RunAsync();
            if (response.IsFailure)
                throw new Exception($"Response #{i} failed");
        }
    }

    /// <summary>
    /// Benchmarks HTTP upload requests with payload data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Benchmark]
    public async Task UploadAsync()
    {
        for (var i = 0; i < TotalRequests; i++)
        {
            var request = _httpRequestFactory
                .New(_serverUri)
                .Get("/upload")
                .Attach(new ByteArrayContent(Helper.GetContent(Size)));
            var response = await request.RunAsync();
            if (response.IsFailure)
                throw new Exception($"Response #{i} failed");
        }
    }

    /// <summary>
    /// Benchmarks HTTP download requests.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Benchmark]
    public async Task DownloadAsync()
    {
        for (var i = 0; i < TotalRequests; i++)
        {
            var request = _httpRequestFactory.New(_serverUri).Get("/download").Param("size", Size);
            var response = await request.RunAsync();
            if (response.IsFailure)
                throw new Exception($"Response #{i} failed");
        }
    }
}
