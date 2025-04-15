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
    protected readonly IReadOnlyList<string> Cmd;
    protected readonly ProcessStartInfo StartInfo;
    private bool _isSensitive;
    private bool _print;

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

    public IShellInstance Print(bool print)
    {
        _print = print;

        return this;
    }

    public IShellInstance MarkSensitive(bool isSensitive = true)
    {
        _isSensitive = isSensitive;

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
        if (!_isSensitive)
            this.Trace<string, string, string>(
                "shell: [{dir}] {fileName} {arguments}",
                process.StartInfo.WorkingDirectory,
                process.StartInfo.FileName,
                process.StartInfo.Arguments
            );

        var tcs = new TaskCompletionSource<ShellResult>();

        // as far as there's no way to know if process was killed or finished on it's own - track it manually
        var killed = false;

        // this will be called when process finished on it's own, or is killed
        var exitHandled = false;

        // setup output capture
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        // track token cancellation and kill process if requested
        var registration = ct.Register(() =>
        {
            killed = true;
            this.Trace<string>("Kill process {command} due token cancellation", GetCommand(process));
            try
            {
                process.Kill();
            }
            catch (Exception ex)
            {
                this.Warn("Kill process {command} failed: {e}", GetCommand(process), ex);
            }

            HandleExit();
        });

        process.Exited += (_, _) =>
        {
            registration.Dispose();
            HandleExit();
        };

        process.Start();

        // setup output capture
        PipeOut(process.StandardOutput, stdout, Console.Out, _print, ct);
        PipeOut(process.StandardError, stderr, Console.Error, _print, ct);

        return tcs;

        void HandleExit()
        {
            if (exitHandled)
                return;
            exitHandled = true;

            if (killed)
                tcs.TrySetCanceled();
            else
                tcs.TrySetResult(GetResult(process.ExitCode, stdout, stderr));
            try
            {
                process.Dispose();
            }
            catch (Exception ex)
            {
                this.Warn("Process.Dispose() failed: {e}", ex);
            }
        }

        static void PipeOut(StreamReader src, StringBuilder result, TextWriter dst, bool print, CancellationToken ct)
        {
            Task.Run(() =>
                {
                    if (print)
                        while (!src.EndOfStream && !ct.IsCancellationRequested)
                        {
                            var c = (char)src.Read();
                            result.Append(c);
                            dst.Write(c);
                        }
                    else
                        while (!src.EndOfStream && !ct.IsCancellationRequested)
                            result.Append((char)src.Read());
                })
                .ConfigureAwait(false)
                .GetAwaiter();
        }

        static ShellResult GetResult(int exitCode, StringBuilder stdout, StringBuilder stderr)
        {
            var output = stdout.ToString();
            var error = stderr.ToString();

            return new ShellResult(exitCode, output, error);
        }
    }

    private string GetCommand(Process process) =>
        $"{process.StartInfo.FileName} {string.Join(' ', process.StartInfo.Arguments)}";
}
