using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Shell;

/// <summary>
/// Interface for configuring and executing a shell command instance
/// </summary>
public interface IShellInstance
{
    /// <summary>
    /// Configures the process start info for the shell command
    /// </summary>
    /// <param name="configure">The configuration action</param>
    /// <returns>The shell instance for method chaining</returns>
    IShellInstance Configure(Action<ProcessStartInfo> configure);

    /// <summary>
    /// Sets whether to print command output to console
    /// </summary>
    /// <param name="print">True to print output, false otherwise</param>
    /// <returns>The shell instance for method chaining</returns>
    IShellInstance Print(bool print);

    /// <summary>
    /// Runs the shell command with a timeout
    /// </summary>
    /// <param name="timeout">The maximum execution time</param>
    /// <returns>The shell execution result</returns>
    Task<ShellResult> RunAsync(TimeSpan timeout);

    /// <summary>
    /// Runs the shell command with cancellation support
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The shell execution result</returns>
    Task<ShellResult> RunAsync(CancellationToken ct = default);

    /// <summary>
    /// Starts the shell command asynchronously without waiting for completion
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>An async result for monitoring the execution</returns>
    ShellAsyncResult Start(CancellationToken ct = default);
}
