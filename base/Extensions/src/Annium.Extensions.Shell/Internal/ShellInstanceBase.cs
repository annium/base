using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Shell.Internal;

/// <summary>
/// Base class for platform-specific shell instance implementations
/// </summary>
internal abstract class ShellInstanceBase : IShellInstance, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for shell operations
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The command and arguments to execute
    /// </summary>
    protected readonly IReadOnlyList<string> Cmd;

    /// <summary>
    /// Process start configuration information
    /// </summary>
    protected readonly ProcessStartInfo StartInfo;

    /// <summary>
    /// Indicates whether the command contains sensitive information that should not be logged
    /// </summary>
    private bool _isSensitive;

    /// <summary>
    /// Indicates whether command output should be printed to console
    /// </summary>
    private bool _print;

    /// <summary>
    /// Initializes a new instance of the shell command base
    /// </summary>
    /// <param name="cmd">The command and arguments to execute</param>
    /// <param name="logger">The logger instance for shell operations</param>
    protected ShellInstanceBase(IReadOnlyList<string> cmd, ILogger logger)
    {
        Cmd = cmd;
        Logger = logger;
        StartInfo = new ProcessStartInfo();
    }

    /// <summary>
    /// Configures the process start info for the shell command
    /// </summary>
    /// <param name="configure">Action to configure the ProcessStartInfo</param>
    /// <returns>The shell instance for method chaining</returns>
    public IShellInstance Configure(Action<ProcessStartInfo> configure)
    {
        configure(StartInfo);

        return this;
    }

    /// <summary>
    /// Sets whether to print command output to console during execution
    /// </summary>
    /// <param name="print">True to print output to console, false to capture only</param>
    /// <returns>The shell instance for method chaining</returns>
    public IShellInstance Print(bool print)
    {
        _print = print;

        return this;
    }

    /// <summary>
    /// Marks the command as containing sensitive information to prevent logging
    /// </summary>
    /// <param name="isSensitive">True to mark as sensitive and prevent logging, false otherwise</param>
    /// <returns>The shell instance for method chaining</returns>
    public IShellInstance MarkSensitive(bool isSensitive = true)
    {
        _isSensitive = isSensitive;

        return this;
    }

    /// <summary>
    /// Executes the shell command with a specified timeout
    /// </summary>
    /// <param name="timeout">Maximum time to wait for command completion</param>
    /// <returns>The result of the shell command execution</returns>
    public async Task<ShellResult> RunAsync(TimeSpan timeout)
    {
        if (timeout == TimeSpan.Zero)
            return await RunAsync(CancellationToken.None);

        using var cts = new CancellationTokenSource(timeout);

        return await RunAsync(cts.Token);
    }

    /// <summary>
    /// Executes the shell command with cancellation support
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>The result of the shell command execution</returns>
    public async Task<ShellResult> RunAsync(CancellationToken ct = default)
    {
        using var process = GetProcess();

        return await StartProcess(process, ct).Task;
    }

    /// <summary>
    /// Starts the shell command asynchronously without waiting for completion
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>An async result containing streams and completion task</returns>
    public ShellAsyncResult Start(CancellationToken ct = default)
    {
        var process = GetProcess();

        var result = StartProcess(process, ct).Task;

        return new ShellAsyncResult(process.StandardInput, process.StandardOutput, process.StandardError, result);
    }

    /// <summary>
    /// Creates and configures a platform-specific process for command execution
    /// </summary>
    /// <returns>A configured Process instance ready for execution</returns>
    protected abstract Process GetProcess();

    /// <summary>
    /// Starts the process and sets up monitoring for completion and cancellation
    /// </summary>
    /// <param name="process">The process to start and monitor</param>
    /// <param name="ct">Cancellation token for operation cancellation</param>
    /// <returns>A task completion source that will complete when the process exits</returns>
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

        // gate for exactly-once HandleExit invocation; both the CT-registration callback
        // and the process.Exited event can fire concurrently — Interlocked.CompareExchange
        // ensures only the first entry proceeds
        var exitHandledFlag = 0;

        // setup output capture
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        // pipe tasks are assigned after process.Start(); HandleExit may fire from either
        // the cancellation token or the Exited event and must await them before reporting
        // the result — initialise to CompletedTask so the closure sees a non-null task even
        // in the extremely rare case of exit firing before pipe setup
        Task stdoutTask = Task.CompletedTask;
        Task stderrTask = Task.CompletedTask;

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

        // setup output capture — capture the pipe tasks so HandleExit can await them
        // before reporting the result; prevents stdout/stderr truncation when the process
        // exits while pipe drains are still in flight
        stdoutTask = PipeOutAsync(process.StandardOutput, stdout, Console.Out, _print, ct);
        stderrTask = PipeOutAsync(process.StandardError, stderr, Console.Error, _print, ct);

        return tcs;

        void HandleExit()
        {
            if (Interlocked.CompareExchange(ref exitHandledFlag, 1, 0) != 0)
                return;

            // background work so we don't block the cancellation-token callback or Process.Exited
            // event; the tcs is only completed after both pipes drain
            _ = Task.Run(async () =>
            {
                try
                {
#pragma warning disable VSTHRD003
                    await Task.WhenAll(stdoutTask, stderrTask);
#pragma warning restore VSTHRD003
                }
                catch
                {
                    // drain failures should not suppress result reporting — the captured bytes so
                    // far are still valuable
                }

                if (killed)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(GetResult(process.ExitCode, stdout, stderr));

                try
                {
                    await process.DisposeAsync();
                }
                catch (Exception ex)
                {
                    this.Warn("Process.Dispose() failed: {e}", ex);
                }
            });
        }

        static Task PipeOutAsync(
            StreamReader src,
            StringBuilder result,
            TextWriter dst,
            bool print,
            CancellationToken ct
        )
        {
            return Task.Run(() =>
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
            });
        }

        static ShellResult GetResult(int exitCode, StringBuilder stdout, StringBuilder stderr)
        {
            var output = stdout.ToString();
            var error = stderr.ToString();

            return new ShellResult(exitCode, output, error);
        }
    }

    /// <summary>
    /// Gets a formatted command string for logging purposes
    /// </summary>
    /// <param name="process">The process to get command string for</param>
    /// <returns>A formatted command string with filename and arguments</returns>
    private string GetCommand(Process process) =>
        $"{process.StartInfo.FileName} {string.Join(' ', process.StartInfo.Arguments)}";
}
