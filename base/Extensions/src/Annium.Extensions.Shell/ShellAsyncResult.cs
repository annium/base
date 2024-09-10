using System.IO;
using System.Threading.Tasks;

namespace Annium.Extensions.Shell;

public sealed record ShellAsyncResult(
    StreamWriter Input,
    StreamReader Output,
    StreamReader Error,
    Task<ShellResult> Result
);
