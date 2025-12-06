using Annium.Net.WebSockets.Benchmark.Internal;
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

WorkloadServer.Start();

var config = new ManualConfig()
    .AddExporter(MarkdownExporter.Default)
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddJob(
        Job.Default.WithWarmupCount(2)
            .WithLaunchCount(3)
            .WithIterationCount(5)
            .WithStrategy(RunStrategy.Throughput)
            .WithPlatform(Platform.AnyCpu)
            .WithRuntime(CoreRuntime.Latest)
    )
    .AddValidator(JitOptimizationsValidator.DontFailOnError)
    .AddLogger(ConsoleLogger.Default)
    .AddColumnProvider(DefaultColumnProviders.Instance);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

await WorkloadServer.StopAsync();
