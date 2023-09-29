namespace Annium.Extensions.Shell;

public class ShellResult
{
    public bool IsSuccess => Code == 0;
    public int Code { get; }
    public string Output { get; }
    public string Error { get; }

    public ShellResult(int code, string output, string error)
    {
        Code = code;
        Output = output;
        Error = error;
    }
}