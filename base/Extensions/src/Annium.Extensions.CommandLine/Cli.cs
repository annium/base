using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Linq;

namespace Annium.Extensions.CommandLine;

/// <summary>
/// Provides utility methods for command line interface operations
/// </summary>
public static class Cli
{
    /// <summary>
    /// Clears the console screen
    /// </summary>
    public static void Clear()
    {
        Console.SetCursorPosition(0, 0);
        var clr = Enumerable
            .Range(0, Console.WindowHeight)
            .Select(_ => new string(' ', Console.WindowWidth))
            .Join(Environment.NewLine);
        Console.Write(clr);
        Console.SetCursorPosition(0, 0);
    }

    /// <summary>
    /// Prompts user for confirmation with Y/N input
    /// </summary>
    /// <param name="label">The confirmation prompt text</param>
    /// <param name="defaultValue">The default value if Enter is pressed</param>
    /// <returns>True if user confirms, false otherwise</returns>
    public static bool Confirm(string label, bool? defaultValue = null)
    {
        var y = defaultValue.HasValue && defaultValue.Value ? 'Y' : 'y';
        var n = defaultValue.HasValue && !defaultValue.Value ? 'N' : 'n';

        Console.WriteLine($"{label} ({y}/{n})");

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Y)
                return true;
            if (key.Key == ConsoleKey.N)
                return false;
            if (defaultValue.HasValue && key.Key == ConsoleKey.Enter)
                return defaultValue.Value;
        }
    }

    /// <summary>
    /// Prompts user for text input
    /// </summary>
    /// <param name="label">The prompt text</param>
    /// <returns>The user input text</returns>
    public static string Prompt(string label)
    {
        Console.Write(label);

        return Console.ReadLine() ?? string.Empty;
    }

    /// <summary>
    /// Reads secure input from user with masked characters
    /// </summary>
    /// <param name="label">The prompt text</param>
    /// <returns>The secure input text</returns>
    public static string ReadSecure(string label)
    {
        Console.Write(label);
        var result = new Stack<char>();
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (result.Count > 0)
                    {
                        result.Pop();
                        var (left, top) = StepBack(Console.CursorLeft, Console.CursorTop, Console.BufferWidth);
                        Console.SetCursorPosition(left, top);
                        Console.Write(' ');
                        Console.SetCursorPosition(left, top);
                    }

                    break;
                case ConsoleKey.Enter:
                    break;
                default:
                    // arrows, Tab, Escape and the like report a control character - usually '\0'. Taking
                    // those as input puts a character the user never typed into the secret, masked as a '*'
                    // like any other, so nothing on screen says the value is not what was typed
                    if (char.IsControl(key.KeyChar))
                        break;

                    result.Push(key.KeyChar);
                    Console.Write('*');
                    break;
            }
        }
        // until enter is pressed
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();

        return string.Join(string.Empty, result.Reverse());
    }

    /// <summary>
    /// Returns the position one character back from the given one, going up a row when it is at the start
    /// of one. A secret longer than the terminal is wide wraps as it is typed, and a column counter that
    /// keeps growing past the buffer width is not a position the console will accept.
    /// </summary>
    /// <param name="left">Current cursor column.</param>
    /// <param name="top">Current cursor row.</param>
    /// <param name="width">Console buffer width.</param>
    /// <returns>The column and row one character back, or the same position when already at the start.</returns>
    internal static (int Left, int Top) StepBack(int left, int top, int width)
    {
        if (left > 0)
            return (left - 1, top);

        // at the start of the very first row there is nothing behind to step onto
        return top > 0 ? (width - 1, top - 1) : (0, 0);
    }

    /// <summary>
    /// Writes colored text to the console
    /// </summary>
    /// <param name="text">The text to write</param>
    /// <param name="foreground">The foreground color</param>
    /// <param name="background">The background color</param>
    public static void WriteColored(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        using var _ = SetColors(foreground, background);
        Console.Write(text);
    }

    /// <summary>
    /// Writes colored text line to the console
    /// </summary>
    /// <param name="text">The text to write</param>
    /// <param name="foreground">The foreground color</param>
    /// <param name="background">The background color</param>
    public static void WriteLineColored(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        using var _ = SetColors(foreground, background);
        Console.WriteLine(text);
    }

    /// <summary>
    /// Sets console colors and returns a disposable to restore original colors
    /// </summary>
    /// <param name="foreground">The foreground color</param>
    /// <param name="background">The background color</param>
    /// <returns>A disposable that restores the original colors when disposed</returns>
    public static IDisposable SetColors(ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        var originalBackground = Console.BackgroundColor;
        if (background.HasValue)
            Console.BackgroundColor = background.Value;

        var originalForeground = Console.ForegroundColor;
        if (foreground.HasValue)
            Console.ForegroundColor = foreground.Value;

        return Disposable.Create(() =>
        {
            Console.BackgroundColor = originalBackground;
            Console.ForegroundColor = originalForeground;
        });
    }
}
