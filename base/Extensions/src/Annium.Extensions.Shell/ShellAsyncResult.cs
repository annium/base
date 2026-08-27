using System.IO;
using System.Threading.Tasks;

namespace Annium.Extensions.Shell;

/// <summary>
/// Represents the result of an asynchronously started shell command
/// </summary>
/// <param name="Input">The standard input stream for writing to the shell command</param>
/// <param name="Result">A task that completes when the shell command finishes execution</param>
/// <remarks>
/// The command's output and error streams are not exposed: they are drained internally into the
/// <see cref="ShellResult"/> this completes with. Handing them out as well would leave two readers racing
/// for the same bytes, splitting the output unpredictably between them.
/// </remarks>
public sealed record ShellAsyncResult(StreamWriter Input, Task<ShellResult> Result);
