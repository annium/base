using System.Threading;
using Annium.Net.Http.Benchmark.Internal;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

var cts = new CancellationTokenSource();
var serverTask = WorkloadServer.RunAsync(cts.Token);

var config = new ManualConfig()
    .AddExporter(MarkdownExporter.Default)
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddJob(Job.Default
        .WithWarmupCount(1)
        .WithLaunchCount(3)
        .WithIterationCount(5)
        .WithStrategy(RunStrategy.Throughput)
        .WithPlatform(Platform.X64)
        .WithRuntime(CoreRuntime.Core70))
    .AddValidator(JitOptimizationsValidator.DontFailOnError)
    .AddLogger(ConsoleLogger.Default)
    .AddColumnProvider(DefaultColumnProviders.Instance);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

cts.Cancel();
await serverTask;