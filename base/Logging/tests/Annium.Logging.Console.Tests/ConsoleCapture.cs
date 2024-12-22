using System;
using System.IO;
using System.Threading;
using SysConsole = System.Console;

namespace Annium.Logging.Console.Tests;

internal class ConsoleCapture : IDisposable
{
    private static readonly Lock _locker = new();

    public static ConsoleCapture Start() => new();

    public string Output => _writer.ToString();

    private readonly TextWriter _stdout;

    private readonly StringWriter _writer = new();

    private ConsoleCapture()
    {
        _locker.Enter();
        _stdout = SysConsole.Out;
        SysConsole.SetOut(_writer);
    }

    public void Dispose()
    {
        SysConsole.SetOut(_stdout);
        _writer.Dispose();
        _locker.Exit();
    }
}
