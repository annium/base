using System.IO;
using System.Threading.Tasks;

namespace Annium.Extensions.Shell;

/// <summary>
/// Represents the result of an asynchronously started shell command with access to input/output streams
/// </summary>
/// <param name="Input">The standard input stream for writing to the shell command</param>
/// <param name="Output">The standard output stream for reading command output</param>
/// <param name="Error">The standard error stream for reading command error output</param>
/// <param name="Result">A task that completes when the shell command finishes execution</param>
public sealed record ShellAsyncResult(
    StreamWriter Input,
    StreamReader Output,
    StreamReader Error,
    Task<ShellResult> Result
);
