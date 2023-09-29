using System.IO;
using System.Threading.Tasks;

namespace Annium.Extensions.Shell;

public class ShellAsyncResult
{
    public StreamWriter Input { get; }
    public StreamReader Output { get; }
    public StreamReader Error { get; }
    public Task<ShellResult> Result { get; }

    public ShellAsyncResult(
        StreamWriter input,
        StreamReader output,
        StreamReader error,
        Task<ShellResult> result
    )
    {
        Input = input;
        Output = output;
        Error = error;
        Result = result;
    }
}