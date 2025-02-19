using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Shell.Internal;

internal abstract class ShellInstanceBase : IShellInstance, ILogSubject
{
    public ILogger Logger { get; }
    private static readonly Lock _locker = new();
    protected readonly IReadOnlyList<string> Cmd;
    protected readonly ProcessStartInfo StartInfo;
    private bool _pipe;

    protected ShellInstanceBase(IReadOnlyList<string> cmd, ILogger logger)
    {
        Cmd = cmd;
        Logger = logger;
        StartInfo = new ProcessStartInfo();
    }

    public IShellInstance Configure(Action<ProcessStartInfo> configure)
    {
        configure(StartInfo);

        return this;
    }

    public IShellInstance Pipe(bool pipe)
    {
        _pipe = pipe;

        return this;
    }

    public async Task<ShellResult> RunAsync(TimeSpan timeout)
    {
        if (timeout == TimeSpan.Zero)
            return await RunAsync(CancellationToken.None);

        using var cts = new CancellationTokenSource(timeout);

        return await RunAsync(cts.Token);
    }

    public async Task<ShellResult> RunAsync(CancellationToken ct = default)
    {
        using var process = GetProcess();

        return await StartProcess(process, ct).Task;
    }

    public ShellAsyncResult Start(CancellationToken ct = default)
    {
        var process = GetProcess();

        var result = StartProcess(process, ct).Task;

        return new ShellAsyncResult(process.StandardInput, process.StandardOutput, process.StandardError, result);
    }

    protected abstract Process GetProcess();

    private TaskCompletionSource<ShellResult> StartProcess(Process process, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<ShellResult>();

        // as far as there's no way to know if process was killed or finished on it's own - track it manually
        var killed = false;

        // this will be called when process finished on it's own, or is killed
        var exitHandled = false;

        // track token cancellation and kill process if requested
        var registration = ct.Register(() =>
        {
            killed = true;
            this.Trace<string>("Kill process {command} due token cancellation", GetCommand(process));
            try
            {
                process.Kill();
            }
            catch (Exception e)
            {
                this.Warn("Kill process {command} failed: {e}", GetCommand(process), e);
            }

            HandleExit();
        });

        process.Exited += (_, _) =>
        {
            registration.Dispose();
            HandleExit();
        };

        process.Start();

        if (_pipe)
        {
            Task.Run(() =>
                {
                    lock (_locker)
                        PipeOut(process.StandardOutput);
                })
                .ConfigureAwait(false)
                .GetAwaiter();
            Task.Run(() =>
                {
                    lock (_locker)
                        PipeOut(process.StandardError);
                })
                .ConfigureAwait(false)
                .GetAwaiter();
        }

        return tcs;

        void HandleExit()
        {
            if (exitHandled)
                return;
            exitHandled = true;

            if (killed)
                tcs.TrySetCanceled();
            else
                tcs.TrySetResult(GetResult(process));
            try
            {
                process.Dispose();
            }
            catch (Exception e)
            {
                this.Warn("Process.Dispose() failed: {e}", e);
            }
        }

        static void PipeOut(StreamReader src)
        {
            while (!src.EndOfStream)
                Console.Write((char)src.Read());
        }
    }

    private ShellResult GetResult(Process process)
    {
        var output = Read(process.StandardOutput);
        var error = Read(process.StandardError);

        return new ShellResult(process.ExitCode, output, error);

        static string Read(StreamReader src)
        {
            var sb = new StringBuilder();
            string? line;
            while ((line = src.ReadLine()) != null)
                sb.AppendLine(line);

            return sb.ToString();
        }
    }

    private string GetCommand(Process process) =>
        $"{process.StartInfo.FileName} {string.Join(' ', process.StartInfo.Arguments)}";
}
