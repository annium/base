using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Execution.Internal.Background;

// ReSharper disable once UnusedTypeParameter
internal class SequentialBackgroundExecutor<TSource> : BackgroundExecutorBase
{
    private int Count => _taskReader.CanCount ? _taskReader.Count : -1;
    private readonly ChannelWriter<Delegate> _taskWriter;
    private readonly ChannelReader<Delegate> _taskReader;
    private ConfiguredTaskAwaitable _runTask = Task.CompletedTask.ConfigureAwait(false);
    private readonly CancellationTokenSource _cts = new();

    public SequentialBackgroundExecutor(
        ILogger logger
    ) : base(logger)
    {
        var taskChannel = Channel.CreateUnbounded<Delegate>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = true,
            SingleWriter = false,
            SingleReader = true
        });
        _taskWriter = taskChannel.Writer;
        _taskReader = taskChannel.Reader;
    }

    protected override void HandleStart()
    {
        _runTask = Task.Run(Run, CancellationToken.None).ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void HandleStop()
    {
        _cts.Cancel();
        _taskWriter.Complete();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ScheduleTaskCore(Delegate task)
    {
        if (!_taskWriter.TryWrite(task))
            throw new InvalidOperationException("Task must have been scheduled");
    }

    protected override async ValueTask HandleDisposeAsync()
    {
        this.Trace("wait for {count} task(s) to finish", Count);
        await _runTask;
        this.Trace("wait for reader completion");
        await _taskReader.Completion.ConfigureAwait(false);
        _cts.Dispose();
    }

    private async Task Run()
    {
        this.Trace("start");
        // normal mode - runs task immediately or waits for one
        while (IsAvailable)
        {
            try
            {
                var task = await _taskReader.ReadAsync(_cts.Token);
                await Helper.RunTaskInForeground(task, Cts.Token);
            }
            catch (ChannelClosedException)
            {
                this.Trace("channel closed");
                break;
            }
            catch (OperationCanceledException)
            {
                this.Trace("operation canceled");
                break;
            }
        }

        // shutdown mode - runs only left tasks
        this.Trace("run {count} tasks left", Count);
        while (true)
        {
            if (_taskReader.TryRead(out var task))
                await Helper.RunTaskInForeground(task, Cts.Token);
            else
                break;
        }

        this.Trace("done");
    }
}