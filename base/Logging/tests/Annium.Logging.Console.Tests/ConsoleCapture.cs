using System;
using System.IO;
using System.Threading;
using SysConsole = System.Console;

namespace Annium.Logging.Console.Tests;

/// <summary>
/// Helper class to capture console output for testing purposes
/// </summary>
internal class ConsoleCapture : IDisposable
{
    /// <summary>
    /// Lock to ensure thread-safe console capture
    /// </summary>
    private static readonly Lock _locker = new();

    /// <summary>
    /// Creates a new console capture instance and starts capturing
    /// </summary>
    /// <returns>A new ConsoleCapture instance</returns>
    public static ConsoleCapture Start() => new();

    /// <summary>
    /// Gets the captured console output
    /// </summary>
    public string Output => _writer.ToString();

    /// <summary>
    /// The original stdout writer
    /// </summary>
    private readonly TextWriter _stdout;

    /// <summary>
    /// The string writer used to capture output
    /// </summary>
    private readonly StringWriter _writer = new();

    private ConsoleCapture()
    {
        _locker.Enter();
        _stdout = SysConsole.Out;
        SysConsole.SetOut(_writer);
    }

    /// <summary>
    /// Restores the original console output and releases resources
    /// </summary>
    public void Dispose()
    {
        SysConsole.SetOut(_stdout);
        _writer.Dispose();
        _locker.Exit();
    }
}
